using Prism.Mvvm;
using System.Data.SQLite;
using System.Reflection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// ＋/－で表示するListViewのアイテム(製品用)
    /// </summary>
    public class ProductDetailsListItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 製品数(モジュール追加用)
        /// </summary>
        private long _Amount;

        /// <summary>
        /// モジュール数
        /// </summary>
        private long _ModuleCount;

        /// <summary>
        /// 最大生産性
        /// </summary>
        private readonly double _MaxEfficiency;

        /// <summary>
        /// 生産性
        /// </summary>
        private double _EfficiencyValue;
        #endregion

        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// モジュール名
        /// </summary>
        public string ModuleName { get; }


        /// <summary>
        /// モジュール数
        /// </summary>
        public long ModuleCount
        {
            get => _ModuleCount;
            set
            {
                if (SetProperty(ref _ModuleCount, value))
                {
                    RaisePropertyChanged(nameof(Amount));
                }
            }
        }


        /// <summary>
        /// 生産性(効率)
        /// </summary>
        public string Efficiency => (_MaxEfficiency < 0) ? "-" : $"{(int)((_MaxEfficiency * _EfficiencyValue + 1.0) * 100)}%";


        /// <summary>
        /// 生産性
        /// </summary>
        public double EfficiencyValue
        {
            get => _EfficiencyValue;
            set
            {
                if (0 < _MaxEfficiency && SetProperty(ref _EfficiencyValue, value))
                {
                    RaisePropertyChanged(nameof(Efficiency));
                    RaisePropertyChanged(nameof(Amount));
                }
            }
        }


        /// <summary>
        /// 製品数
        /// </summary>
        public long Amount => (long)((_MaxEfficiency * _EfficiencyValue + 1.0) * _Amount * ModuleCount);


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="moduleCount">モジュール数</param>
        /// <param name="efficiency">効率</param>
        /// <param name="amount">製品数</param>
        public ProductDetailsListItem(string moduleID, long moduleCount, double efficiency, long amount)
        {
            ModuleID = moduleID;
            ModuleCount = moduleCount;
            _Amount = amount;
            _MaxEfficiency = efficiency;

            ModuleName = DB.X4DB.Module.Get(moduleID).Name;
        }

        /// <summary>
        /// モジュールが増えたことにする
        /// </summary>
        /// <param name="count">増分量</param>
        public void Incriment(long count)
        {
            ModuleCount += count;
        }
    }
}
