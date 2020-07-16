using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// 製品一覧を表示するDataGridViewの1レコード分用クラス
    /// </summary>
    public class ProductsGridItem : BindableBase
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
        /// 採掘船を割り当て
        /// </summary>
        private bool _IsAssignMiner;

        /// <summary>
        /// 不足リソースを他ステーションから供給
        /// </summary>
        private bool _IsSupplyingScareResourcesFromOtherStations;
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
        /// 金額
        /// </summary>
        public long Price
        {
            get
            {
                var ret = UnitPrice * Count;

                // 購入が必要だが、他の手段で供給を受けられる場合金額を0にする
                if (ret < 0 && (IsAssignMiner || IsSupplyingScareResourcesFromOtherStations))
                {
                    ret = 0;
                }

                return ret;
            }
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

                // 変更無しの場合は何もしない
                if (_UnitPrice == value)
                {
                    return;
                }


                if (value < Ware.MinPrice)
                {
                    // 入力された値が最低価格未満の場合、最低価格を設定する
                    _UnitPrice = Ware.MinPrice;
                }
                else if (Ware.MaxPrice < value)
                {
                    // 入力された値が最高価格を超える場合、最高価格を設定する
                    _UnitPrice = Ware.MaxPrice;
                }
                else
                {
                    // 入力された値が最低価格以上、最高価格以下の場合、入力された値を設定する
                    _UnitPrice = value;
                }
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Price));
            }
        }

        /// <summary>
        /// ウェア詳細(関連モジュール等)
        /// </summary>
        public ObservableRangeCollection<ProductDetailsListItem> Details { get; }


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
        /// 採掘船を割り当て
        /// </summary>
        public bool IsAssignMiner
        {
            get => _IsAssignMiner;
            set
            {
                // 採掘船の収集対象以外？
                if (0 < Ware.WareGroup.Tier)
                {
                    throw new InvalidOperationException();
                }

                if (SetProperty(ref _IsAssignMiner, value))
                {
                    // 不足リソースを他ステーションから供給ONの場合、価格は既に0なのでPriceの変更通知しない
                    if (IsSupplyingScareResourcesFromOtherStations && value)
                    {
                        return;
                    }

                    RaisePropertyChanged(nameof(Price));
                }
            }
        }


        /// <summary>
        /// 不足リソースを他ステーションから供給
        /// </summary>
        public bool IsSupplyingScareResourcesFromOtherStations
        {
            get => _IsSupplyingScareResourcesFromOtherStations;
            set
            {
                if (SetProperty(ref _IsSupplyingScareResourcesFromOtherStations, value))
                {
                    // 採掘船を割り当てONの場合、価格は既に0なのでPriceの変更通知しない
                    if (IsAssignMiner)
                    {
                        return;
                    }

                    RaisePropertyChanged(nameof(Price));
                }
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="count">ウェア個数</param>
        /// <param name="datails">ウェア詳細(関連モジュール等)</param>
        /// <param name="isExpanded">関連モジュールが展開されているか</param>
        /// <param name="price">価格</param>
        public ProductsGridItem(string wareID, IEnumerable<ProductDetailsListItem> datails)
        {
            Ware = Ware.Get(wareID);
            UnitPrice = (Ware.MinPrice + Ware.MaxPrice) / 2;
            Details = new ObservableRangeCollection<ProductDetailsListItem>(datails);
        }


        /// <summary>
        /// 詳細情報を追加
        /// </summary>
        /// <param name="details"></param>
        public void AddDetails(IEnumerable<ProductDetailsListItem> details)
        {
            var addItems = new List<ProductDetailsListItem>();

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

            RaisePropertyChanged(nameof(Count));
            RaisePropertyChanged(nameof(Price));
        }

        /// <summary>
        /// 詳細情報を設定
        /// </summary>
        /// <param name="details"></param>
        public void SetDetails(IEnumerable<ProductDetailsListItem> details, long prevModuleCount)
        {
            foreach (var item in details)
            {
                // 更新対象のモジュールを検索
                var tmp = Details.Where(x => x.ModuleID == item.ModuleID).FirstOrDefault();
                if (tmp != null)
                {
                    tmp.ModuleCount += (item.ModuleCount - prevModuleCount);
                }
            }

            RaisePropertyChanged(nameof(Count));
            RaisePropertyChanged(nameof(Price));
        }


        /// <summary>
        /// 詳細情報を削除
        /// </summary>
        /// <param name="details"></param>
        public void RemoveDetails(IEnumerable<ProductDetailsListItem> details)
        {
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

            RaisePropertyChanged(nameof(Count));
            RaisePropertyChanged(nameof(Price));
        }


        /// <summary>
        /// 生産性を設定
        /// </summary>
        /// <param name="efficiency"></param>
        public void SetEfficiency(double efficiency)
        {
            foreach (var item in Details)
            {
                item.EfficiencyValue = efficiency;
            }

            RaisePropertyChanged(nameof(Count));
            RaisePropertyChanged(nameof(Price));
        }
    }
}
