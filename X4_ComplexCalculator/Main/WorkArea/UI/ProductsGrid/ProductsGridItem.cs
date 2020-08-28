using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// 製品一覧を表示するDataGridViewの1レコード分用クラス
    /// </summary>
    public class ProductsGridItem : BindableBaseEx, IEditable
    {
        #region メンバ
        /// <summary>
        /// 単価
        /// </summary>
        private long _UnitPrice;


        /// <summary>
        /// Expanderが展開されているか
        /// </summary>
        private bool _IsExpanded;


        /// <summary>
        /// 売買オプション
        /// </summary>
        readonly TradeOption _TradeOption;


        /// <summary>
        /// 編集状態
        /// </summary>
        private EditStatus _EditStatus = EditStatus.Unedited;
        #endregion


        #region プロパティ
        /// <summary>
        /// 製品
        /// </summary>
        public Ware Ware { get; }


        /// <summary>
        /// ウェアの個数
        /// </summary>
        public long Count => Details.Sum(x => x.Amount);


        /// <summary>
        /// 価格
        /// </summary>
        public long Price
        {
            // ウェアが不足しているが購入しない or ウェアが余っているが販売しない場合、価格を0にする
            get => (Count < 0 && NoBuy) || (0 < Count && NoSell) ? 0 : UnitPrice * Count;
        }


        /// <summary>
        /// 単価
        /// </summary>
        public long UnitPrice
        {
            get => _UnitPrice;
            set
            {
                // 最低価格≦ 入力価格 ≦ 最高価格かつ価格が変更された場合のみ更新


                var setValue = value;

                if (setValue < Ware.MinPrice)
                {
                    // 入力された値が最低価格未満の場合、最低価格を設定する
                    setValue = Ware.MinPrice;
                }
                else if (Ware.MaxPrice < setValue)
                {
                    // 入力された値が最高価格を超える場合、最高価格を設定する
                    setValue = Ware.MaxPrice;
                }

                // 変更無しの場合は何もしない
                if (setValue == _UnitPrice)
                {
                    return;
                }

                var oldUnitPrice = _UnitPrice;
                var oldPrice = Price;
                _UnitPrice = setValue;

                RaisePropertyChangedEx(oldUnitPrice, setValue);
                RaisePropertyChangedEx(oldPrice, Price, nameof(Price));
                EditStatus = EditStatus.Edited;
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
        public bool IsExpanded
        {
            get => _IsExpanded;
            set => SetProperty(ref _IsExpanded, value);
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
        /// 購入しないか
        /// </summary>
        public bool NoBuy
        {
            get => _TradeOption.NoBuy;
            set
            {
                var oldPrice = Price;

                if (SetProperty(ref _TradeOption.NoBuy, value))
                {
                    if (oldPrice != Price)
                    {
                        RaisePropertyChangedEx(oldPrice, Price, nameof(Price));
                    }
                    EditStatus = EditStatus.Edited;
                }
            }
        }


        /// <summary>
        /// 販売しないか
        /// </summary>
        public bool NoSell
        {
            get => _TradeOption.NoSell;
            set
            {
                var oldPrice = Price;

                if (SetProperty(ref _TradeOption.NoSell, value))
                {
                    if (oldPrice != Price)
                    {
                        RaisePropertyChangedEx(oldPrice, Price, nameof(Price));
                    }
                    EditStatus = EditStatus.Edited;
                }
            }
        }


        /// <summary>
        /// 編集状態
        /// </summary>
        public EditStatus EditStatus
        {
            get => _EditStatus;
            set => SetProperty(ref _EditStatus, value);
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="datails">ウェア詳細(関連モジュール等)</param>
        /// <param name="tradeOption">売買オプション</param>
        public ProductsGridItem(string wareID, IEnumerable<IProductDetailsListItem> datails, TradeOption tradeOption)
        {
            Ware = Ware.Get(wareID);
            Details = new ObservableRangeCollection<IProductDetailsListItem>(datails);

            _TradeOption = tradeOption;
            UnitPrice = (Ware.MinPrice + Ware.MaxPrice) / 2;
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="datails">ウェア詳細(関連モジュール等)</param>
        /// <param name="tradeOption">売買オプション</param>
        /// <param name="unitPrice">単価</param>
        public ProductsGridItem(string wareID, IEnumerable<IProductDetailsListItem> datails, TradeOption tradeOption, long unitPrice)
        {
            Ware = Ware.Get(wareID);
            Details = new ObservableRangeCollection<IProductDetailsListItem>(datails);

            _TradeOption = tradeOption;
            UnitPrice = unitPrice;
        }



        /// <summary>
        /// 詳細情報を追加
        /// </summary>
        /// <param name="details"></param>
        public void AddDetails(IEnumerable<IProductDetailsListItem> details)
        {
            var addItems = new List<IProductDetailsListItem>();

            var oldCount = Count;
            var oldPrice = Price;

            foreach (var item in details)
            {
                var tmp = Details.Where(x => x.ModuleID == item.ModuleID).FirstOrDefault();
                if (tmp != null)
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
                    RaisePropertyChangedEx(oldCount, newCount, nameof(Count));
                }
            }
            {
                var newPrice = Price;
                if (oldPrice != newPrice)
                {
                    RaisePropertyChangedEx(oldPrice, newPrice, nameof(Price));
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
                var tmp = Details.Where(x => x.ModuleID == item.ModuleID).FirstOrDefault();
                if (tmp != null)
                {
                    tmp.ModuleCount += (item.ModuleCount - prevModuleCount);
                }
            }


            {
                var newCount = Count;
                if (oldCount != newCount)
                {
                    RaisePropertyChangedEx(oldCount, newCount, nameof(Count));
                }
            }
            {
                var newPrice = Price;
                if (oldPrice != newPrice)
                {
                    RaisePropertyChangedEx(oldPrice, newPrice, nameof(Price));
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
                var tmp = Details.Where(x => x.ModuleID == item.ModuleID).FirstOrDefault();
                if (tmp != null)
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
                    RaisePropertyChangedEx(oldCount, newCount, nameof(Count));
                }
            }
            {
                var newPrice = Price;
                if (oldPrice != newPrice)
                {
                    RaisePropertyChangedEx(oldPrice, newPrice, nameof(Price));
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
                    RaisePropertyChangedEx(oldCount, newCount, nameof(Count));
                }
            }
            {
                var newPrice = Price;
                if (oldPrice != newPrice)
                {
                    RaisePropertyChangedEx(oldPrice, newPrice, nameof(Price));
                }
            }
        }
    }
}
