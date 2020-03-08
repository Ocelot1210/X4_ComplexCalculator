using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.ResourcesGrid
{
    /// <summary>
    /// 建造に必要なウェアを表示するDataGridViewの1レコード分のクラス
    /// </summary>
    public class ResourcesGridItem : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 単価
        /// </summary>
        private long _UnitPrice;
        #endregion

        #region プロパティ
        /// <summary>
        /// 建造に必要なウェア
        /// </summary>
        public Ware Ware { get; private set; }


        /// <summary>
        /// 建造に必要なウェア量
        /// </summary>
        public long Count { get; }


        /// <summary>
        /// 金額
        /// </summary>
        public long Price => Count * UnitPrice;

        /// <summary>
        /// 単価
        /// </summary>
        public long UnitPrice
        {
            get
            {
                return _UnitPrice;
            }
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
                OnPropertyChanged();
                OnPropertyChanged("Price");
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
        /// <param name="ware">建造に必要なウェアID</param>
        /// <param name="count">建造に必要なウェアの個数</param>
        public ResourcesGridItem(string wareID, long count)
        {
            Ware = new Ware(wareID);
            Count = count;
            UnitPrice = (Ware.MaxPrice + Ware.MinPrice) / 2;
        }
    }
}
