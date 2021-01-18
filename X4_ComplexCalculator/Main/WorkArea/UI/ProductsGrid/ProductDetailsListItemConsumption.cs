using System;
using Prism.Mvvm;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// 製品一覧DataGridの＋/－で表示するListViewのアイテム(消費品)
    /// </summary>
    class ProductDetailsListItemConsumption : BindableBase, IProductDetailsListItem
    {
        #region メンバ
        /// <summary>
        /// 製品数(モジュール追加用)
        /// </summary>
        private readonly long _Amount;


        /// <summary>
        /// モジュール数
        /// </summary>
        private long _ModuleCount;
        #endregion


        #region プロパティ
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
        /// 製品数
        /// </summary>
        public long Amount => _Amount * ModuleCount;


        /// <summary>
        /// 生産性(効率)
        /// </summary>
        public double Efficiency => -1.0;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="moduleCount">モジュール数</param>
        /// <param name="amount">製品数</param>
        public ProductDetailsListItemConsumption(string moduleID, long moduleCount, long amount)
        {
            ModuleID = moduleID;
            ModuleCount = moduleCount;
            _Amount = amount;

            ModuleName = Ware.TryGet<Module>(moduleID)?.Name ?? throw new ArgumentException($"Invalid module ID. ({moduleID}", nameof(moduleID));
        }



        /// <summary>
        /// 生産性を設定
        /// </summary>
        /// <param name="effectID">効果ID</param>
        /// <param name="value">設定値</param>
        public void SetEfficiency(string effectID, double value)
        {
            // 何もしない
        }
    }
}
