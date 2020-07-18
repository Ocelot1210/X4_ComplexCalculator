using Prism.Mvvm;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid
{
    /// <summary>
    /// 建造に必要なウェアを表示するDataGridViewの1レコード分のクラス
    /// </summary>
    public class BuildResourcesGridItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 単価
        /// </summary>
        private long _UnitPrice;

        /// <summary>
        /// 建造に必要なウェア数量
        /// </summary>
        private long _Amount;
        #endregion


        #region プロパティ
        /// <summary>
        /// 建造に必要なウェア
        /// </summary>
        public Ware Ware { get; private set; }


        /// <summary>
        /// 建造に必要なウェア数量
        /// </summary>
        public long Amount
        {
            get => _Amount;
            set
            {
                if (SetProperty(ref _Amount, value))
                {
                    RaisePropertyChanged(nameof(Price));
                }
            }
        }


        /// <summary>
        /// 金額
        /// </summary>
        public long Price => Amount * UnitPrice;


        /// <summary>
        /// 単価
        /// </summary>
        public long UnitPrice
        {
            get => _UnitPrice;
            set
            {
                // 最低価格≦ 入力価格 ≦ 最高価格かつ価格が変更された場合のみ更新
                if (_UnitPrice == value)
                {
                    // 変更無しの場合は何もしない
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
        /// 選択されたか
        /// </summary>
        public bool IsSelected { get; set; }


        /// <summary>
        /// 百分率ベースで価格を設定する
        /// </summary>
        /// <param name="percent">百分率の値</param>
        public void SetUnitPricePercent(long percent)
        {
            UnitPrice = (long)(Ware.MinPrice + (Ware.MaxPrice - Ware.MinPrice) * 0.01 * percent);
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">建造に必要なウェアID</param>
        /// <param name="amount">建造に必要なウェア数</param>
        public BuildResourcesGridItem(string wareID, long amount)
        {
            Ware = Ware.Get(wareID);
            UnitPrice = (Ware.MaxPrice + Ware.MinPrice) / 2;
            Amount = amount;
        }
    }
}
