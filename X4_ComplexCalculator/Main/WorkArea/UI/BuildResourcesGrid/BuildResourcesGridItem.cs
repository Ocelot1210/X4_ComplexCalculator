using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid
{
    /// <summary>
    /// 建造に必要なウェアを表示するDataGridViewの1レコード分のクラス
    /// </summary>
    public class BuildResourcesGridItem : BindableBaseEx, IEditable, ISelectable
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


        /// <summary>
        /// 建造ウェアを購入しない
        /// </summary>
        private bool _NoBuy;


        /// <summary>
        /// 編集状態
        /// </summary>
        private EditStatus _EditStatus = EditStatus.Unedited;
        #endregion


        #region プロパティ
        /// <summary>
        /// 建造に必要なウェア
        /// </summary>
        public Ware Ware { get; }


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
        public long Price => NoBuy ? 0 : Amount * UnitPrice;


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

                if (value < Ware.MinPrice)
                {
                    // 入力された値が最低価格未満の場合、最低価格を設定する
                    setValue = Ware.MinPrice;
                }
                else if (Ware.MaxPrice < value)
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

                if (!NoBuy)
                {
                    RaisePropertyChangedEx(oldPrice, Price, nameof(Price));
                }

                EditStatus = EditStatus.Edited;
            }
        }


        /// <summary>
        /// 選択されているか
        /// </summary>
        public bool IsSelected { get; set; }


        /// <summary>
        /// 建造ウェアを購入しない
        /// </summary>
        public bool NoBuy
        {
            get => _NoBuy;
            set
            {
                var oldPrice = Price;

                if (SetProperty(ref _NoBuy, value))
                {
                    RaisePropertyChangedEx(oldPrice, Price, nameof(Price));
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
        /// <param name="wareID">建造に必要なウェアID</param>
        /// <param name="amount">建造に必要なウェア数</param>
        public BuildResourcesGridItem(string wareID, long amount)
        {
            Ware = Ware.Get(wareID);
            UnitPrice = (Ware.MaxPrice + Ware.MinPrice) / 2;
            Amount = amount;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">建造に必要なウェアID</param>
        /// <param name="amount">建造に必要なウェア数</param>
        /// <param name="unitPrice">単価</param>
        public BuildResourcesGridItem(string wareID, long amount, long unitPrice)
        {
            Ware = Ware.Get(wareID);
            UnitPrice = unitPrice;
            Amount = amount;
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
}
