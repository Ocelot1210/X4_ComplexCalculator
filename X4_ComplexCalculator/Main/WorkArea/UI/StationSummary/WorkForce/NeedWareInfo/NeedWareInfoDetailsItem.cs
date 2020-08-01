using Prism.Mvvm;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.NeedWareInfo
{
    /// <summary>
    /// 必要ウェア詳細情報1レコード分
    /// </summary>
    class NeedWareInfoDetailsItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 必要数量
        /// </summary>
        private long _NeedAmount;


        /// <summary>
        /// 合計必要数量
        /// </summary>
        private long _TotalNeedAmount;


        /// <summary>
        /// 生産数量
        /// </summary>
        private long _ProductionAmount;
        #endregion


        #region プロパティ
        /// <summary>
        /// 種族
        /// </summary>
        public Race Race { get; }


        /// <summary>
        /// 労働方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// 必要ウェア名
        /// </summary>
        public string WareName { get; }


        /// <summary>
        /// 必要数量
        /// </summary>
        public long NeedAmount
        {
            get => _NeedAmount;
            set
            {
                if (SetProperty(ref _NeedAmount, value))
                {
                    RaisePropertyChanged(nameof(Diff));
                }
            }
        }


        /// <summary>
        /// 合計必要数量
        /// </summary>
        public long TotalNeedAmount
        {
            get => _TotalNeedAmount;
            set => SetProperty(ref _TotalNeedAmount, value);
        }


        /// <summary>
        /// 生産数量
        /// </summary>
        public long ProductionAmount
        {
            get => _ProductionAmount;
            set
            {
                if (SetProperty(ref _ProductionAmount, value))
                {
                    RaisePropertyChanged(nameof(Diff));
                }
            }
        }


        /// <summary>
        /// 差
        /// </summary>
        public long Diff => ProductionAmount - NeedAmount;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="race">種族</param>
        /// <param name="method">労働方式</param>
        /// <param name="wareID">ウェアID</param>
        /// <param name="needAmount">必要数量</param>
        /// <param name="productionAmount">生産数量</param>
        public NeedWareInfoDetailsItem(Race race, string method, string wareID, long needAmount = 0, long productionAmount = 0)
        {
            Race             = race;
            Method           = method;
            WareID           = wareID;
            WareName         = Ware.Get(wareID).Name;
            NeedAmount       = needAmount;
            ProductionAmount = productionAmount;
        }
    }
}
