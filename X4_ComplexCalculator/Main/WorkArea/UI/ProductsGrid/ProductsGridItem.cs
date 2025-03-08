using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

/// <summary>
/// 製品一覧を表示するDataGridViewの1レコード分用クラス
/// </summary>
public partial class ProductsGridItem : ObservableRecipientEx, IEditable, ISelectable
{
    #region プロパティ
    /// <summary>
    /// 製品
    /// </summary>
    public IWare Ware { get; }


    /// <summary>
    /// ウェアの個数
    /// </summary>
    public long Count => Details.Sum(x => x.Amount);


    /// <summary>
    /// 価格
    /// </summary>
    /// <remarks>ウェアが不足しているが購入しない or ウェアが余っているが販売しない場合、価格を0にする</remarks>
    public long Price => CalcPrice(Count, UnitPrice, NoBuy, NoSell);


    /// <summary>
    /// 単価
    /// </summary>
    public long UnitPrice
    {
        get => field;
        set
        {
            // 最低価格≦ 入力価格 ≦ 最高価格に抑える
            var oldUnitPrice = field;
            var oldPrice = Price;

            var setValue = Math.Min(Math.Max(value, Ware.MinPrice), Ware.MaxPrice);
            if (SetProperty(ref field, setValue))
            {
                OnPropertyChanged(nameof(UnitPrice));
                OnPropertyChanged(nameof(Price));

                Broadcast(oldUnitPrice, UnitPrice, nameof(UnitPrice));
                Broadcast(oldPrice, Price, nameof(Price));
                EditStatus = EditStatus.Edited;
            }
        }
    }


    /// <summary>
    /// ウェア詳細(関連モジュール等)
    /// </summary>
    public ObservableRangeCollection<IProductDetailsListItem> Details { get; }


    /// <summary>
    /// 選択されているか
    /// </summary>
    public bool IsSelected { get; set; }


    /// <summary>
    /// Expanderが展開されているか
    /// </summary>
    [ObservableProperty]
    public partial bool IsExpanded { get; set; }


    /// <summary>
    /// 購入しないか
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    [NotifyPropertyChangedFor(nameof(Price))]
    public partial bool NoBuy { get; set; }


    /// <summary>
    /// 販売しないか
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    [NotifyPropertyChangedFor(nameof(Price))]
    public partial bool NoSell { get; set; }


    /// <summary>
    /// 編集状態
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial EditStatus EditStatus { get; set; } = EditStatus.Unedited;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="wareID">ウェア</param>
    /// <param name="datails">ウェア詳細(関連モジュール等)</param>
    /// <param name="noBuy">購入しないか</param>
    /// <param name="noSell">販売しないか</param>
    /// <param name="unitPrice">単価</param>
    public ProductsGridItem(IMessenger messenger, IWare ware, IEnumerable<IProductDetailsListItem> datails, bool noBuy = false, bool noSell = false, long unitPrice = -1) : base(messenger)
    {
        Ware = ware;
        Details = new ObservableRangeCollection<IProductDetailsListItem>(datails);

        NoBuy = noBuy;
        NoSell = noSell;

        UnitPrice = unitPrice == -1 ? (Ware.MinPrice + Ware.MaxPrice) / 2 : unitPrice;

        IsActive = true;
    }


    /// <summary>
    /// <see cref="NoBuy"/> 変更時
    /// </summary>
    partial void OnNoBuyChanged(bool oldValue, bool newValue)
    {
        var oldPrice = CalcPrice(Count, UnitPrice, oldValue, NoSell);
        if (Price != oldPrice)
        {
            Broadcast(oldPrice, Price, nameof(Price));
        }
        EditStatus = EditStatus.Edited;
    }


    /// <summary>
    /// <see cref="NoSell"/> 変更時
    /// </summary>
    partial void OnNoBuyChanging(bool oldValue, bool newValue)
    {
        var oldPrice = CalcPrice(Count, UnitPrice, NoBuy, oldValue);
        if (Price != oldPrice)
        {
            Broadcast(oldPrice, Price, nameof(Price));
        }
        EditStatus = EditStatus.Edited;
    }


    /// <summary>
    /// 百分率ベースで価格を設定する
    /// </summary>
    /// <param name="percent">百分率の値</param>
    public void SetUnitPricePercent(long percent)
    {
        UnitPrice = (long)(Ware.MinPrice + (Ware.MaxPrice - Ware.MinPrice) * 0.01 * percent);
    }


    /// <summary>
    /// 詳細情報を追加
    /// </summary>
    /// <param name="details"></param>
    public void AddDetails(IEnumerable<IProductDetailsListItem> details)
    {
        using var addItems = new PooledList<IProductDetailsListItem>();

        var oldCount = Count;
        var oldPrice = Price;

        foreach (var item in details)
        {
            var tmp = Details.FirstOrDefault(x => x.ModuleID == item.ModuleID);
            if (tmp is not null)
            {
                // 既にモジュールがある場合
                tmp.ModuleCount += item.ModuleCount;
            }
            else
            {
                // 初回追加の場合
                addItems.Add(item);
            }
        }

        Details.AddRange(addItems);

        {
            var newCount = Count;
            if (oldCount != newCount)
            {
                OnPropertyChanged(nameof(Count));
                Broadcast(oldCount, newCount, nameof(Count));
            }
        }
        {
            var newPrice = Price;
            if (oldPrice != newPrice)
            {
                OnPropertyChanged(nameof(Price));
                Broadcast(oldPrice, newPrice, nameof(Price));
            }
        }
    }


    /// <summary>
    /// 詳細情報を設定
    /// </summary>
    /// <param name="details"></param>
    public void SetDetails(IEnumerable<IProductDetailsListItem> details, long prevModuleCount)
    {
        var oldCount = Count;
        var oldPrice = Price;

        foreach (var item in details)
        {
            // 更新対象のモジュールを検索
            var tmp = Details.FirstOrDefault(x => x.ModuleID == item.ModuleID);
            if (tmp is not null)
            {
                tmp.ModuleCount += (item.ModuleCount - prevModuleCount);
            }
        }


        {
            var newCount = Count;
            if (oldCount != newCount)
            {
                OnPropertyChanged(nameof(Count));
                Broadcast(oldCount, newCount, nameof(Count));
            }
        }
        {
            var newPrice = Price;
            if (oldPrice != newPrice)
            {
                OnPropertyChanged(nameof(Price));
                Broadcast(oldPrice, newPrice, nameof(Price));
            }
        }
    }


    /// <summary>
    /// 詳細情報を削除
    /// </summary>
    /// <param name="details"></param>
    public void RemoveDetails(IEnumerable<IProductDetailsListItem> details)
    {
        var oldCount = Count;
        var oldPrice = Price;

        foreach (var item in details)
        {
            var tmp = Details.FirstOrDefault(x => x.ModuleID == item.ModuleID);
            if (tmp is not null)
            {
                // 既にモジュールがある場合
                tmp.ModuleCount -= item.ModuleCount;
            }
        }

        // 空のレコードを削除
        Details.RemoveAll(x => x.ModuleCount == 0);

        {
            var newCount = Count;
            if (oldCount != newCount)
            {
                OnPropertyChanged(nameof(Count));
                Broadcast(oldCount, newCount, nameof(Count));
            }
        }
        {
            var newPrice = Price;
            if (oldPrice != newPrice)
            {
                OnPropertyChanged(nameof(Price));
                Broadcast(oldPrice, newPrice, nameof(Price));
            }
        }
    }


    /// <summary>
    /// 生産性を設定
    /// </summary>
    /// <param name="effectID">効果ID</param>
    /// <param name="efficiency">設定値</param>
    public void SetEfficiency(string effectID, double efficiency)
    {
        var oldCount = Count;
        var oldPrice = Price;

        foreach (var item in Details)
        {
            item.SetEfficiency(effectID, efficiency);
        }


        {
            var newCount = Count;
            if (oldCount != newCount)
            {
                OnPropertyChanged(nameof(Count));
                Broadcast(oldCount, newCount, nameof(Count));
            }
        }
        {
            var newPrice = Price;
            if (oldPrice != newPrice)
            {
                OnPropertyChanged(nameof(Price));
                Broadcast(oldPrice, newPrice, nameof(Price));
            }
        }
    }




    /// <summary>
    /// 価格を計算
    /// </summary>
    /// <param name="count">個数</param>
    /// <param name="unitPrice">単価</param>
    /// <param name="noBuy">購入しないフラグ</param>
    /// <param name="noSell">販売しないフラグ</param>
    /// <returns>ウェアが不足しているが購入しない or ウェアが余っているが販売しない場合、0。それ以外の場合、個数×単価</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long CalcPrice(long count, long unitPrice, bool noBuy, bool noSell)
    {
        return (count < 0 && noBuy) || (0 < count && noSell) ? 0 : unitPrice * count;
    }
}
