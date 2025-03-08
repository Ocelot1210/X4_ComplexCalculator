using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;

/// <summary>
/// 建造に必要なウェアを表示するDataGridViewの1レコード分のクラス
/// </summary>
public sealed partial class BuildResourcesGridItem : ObservableRecipientEx, IEditable, ISelectable
{
    #region プロパティ
    /// <summary>
    /// 建造に必要なウェア
    /// </summary>
    public IWare Ware { get; }


    /// <summary>
    /// 建造に必要なウェア数量
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    [NotifyPropertyChangedFor(nameof(Price))]
    public partial long Amount { get; set; }


    /// <summary>
    /// 金額
    /// </summary>
    public long Price => NoBuy ? 0 : Amount * UnitPrice;


    /// <summary>
    /// 単価
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    [NotifyPropertyChangedFor(nameof(Price))]
    public partial long UnitPrice { get; set; }


    /// <summary>
    /// 選択されているか
    /// </summary>
    public bool IsSelected { get; set; }


    /// <summary>
    /// 建造ウェアを購入しない
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    [NotifyPropertyChangedFor(nameof(Price))]
    public partial bool NoBuy { get; set; }


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
    /// <param name="messanger">メッセージ交換用</param>
    /// <param name="wareID">建造に必要なウェアID</param>
    /// <param name="amount">建造に必要なウェア数</param>
    /// <param name="unitPrice">単価</param>
    public BuildResourcesGridItem(IMessenger messenger, string wareID, long amount, long unitPrice = 0) : base(messenger)
    {
        IsActive = false;
        Ware = X4Database.Instance.Ware.Get(wareID);
        UnitPrice = unitPrice != 0 ? unitPrice : (Ware.MaxPrice + Ware.MinPrice) / 2;
        Amount = amount;
        IsActive = true;
    }


    /// <summary>
    /// <see cref="Amount"/> 変更時
    /// </summary>
    partial void OnAmountChanged(long oldValue, long newValue) => SendPriceChangedMessage(oldValue, UnitPrice);


    /// <summary>
    /// <see cref="UnitPrice"/> 変更時
    /// </summary>
    partial void OnUnitPriceChanged(long oldValue, long newValue) => SendPriceChangedMessage(Amount, oldValue);


    /// <summary>
    /// <see cref="Amount"/> または <see cref="UnitPrice"/> 変更に伴う <see cref="Price"/> 変更通知
    /// </summary>
    private void SendPriceChangedMessage(long amount, long unitPrice)
    {
        if (!NoBuy)
        {
            // 建造ウェアを購入する場合は変更前後の金額を通知
            Broadcast(amount * unitPrice, Price, nameof(Price));
        }
        EditStatus = EditStatus.Edited;
    }


    /// <summary>
    /// <see cref="NoBuy"/> 変更時
    /// </summary>
    partial void OnNoBuyChanged(bool oldValue, bool newValue)
    {
        var oldPrice = oldValue ? 0 : Amount * UnitPrice;
        Broadcast(oldPrice, Price, nameof(Price));
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
}
