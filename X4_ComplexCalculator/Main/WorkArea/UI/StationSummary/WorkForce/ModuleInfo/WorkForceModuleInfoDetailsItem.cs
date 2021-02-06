using System;
using Prism.Mvvm;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.ModuleInfo
{
    /// <summary>
    /// 労働力用ListVierのアイテム
    /// </summary>
    public class WorkForceModuleInfoDetailsItem : BindableBase
    {
        #region メンバ
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
                    RaisePropertyChanged(nameof(TotalWorkforce));
                }
            }
        }


        /// <summary>
        /// 必要労働力
        /// </summary>
        public long MaxWorkers { get; }


        /// <summary>
        /// 収容人数
        /// </summary>
        public long WorkersCapacity { get; }


        /// <summary>
        /// 労働者数
        /// </summary>
        public long WorkForce => WorkersCapacity - MaxWorkers;


        /// <summary>
        /// 総労働者数
        /// </summary>
        public long TotalWorkforce => ModuleCount * WorkForce;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">モジュールオブジェクト</param>
        /// <param name="moduleCount">モジュール数</param>
        public WorkForceModuleInfoDetailsItem(IX4Module module, long moduleCount)
        {
            ModuleID = module.ID;
            ModuleName = module.Name;
            _ModuleCount = moduleCount;
            MaxWorkers = module.MaxWorkers;
            WorkersCapacity = module.WorkersCapacity;
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="moduleCount">モジュール数</param>
        /// <param name="maxWorkers">必要従業員数</param>
        /// <param name="workersCapacity">従業員容量</param>
        public WorkForceModuleInfoDetailsItem(string moduleID, long moduleCount, long maxWorkers, long workersCapacity)
        {
            ModuleID = moduleID;
            ModuleName = X4Database.Instance.Ware.Get<IX4Module>(moduleID).Name;
            _ModuleCount = moduleCount;
            MaxWorkers = maxWorkers;
            WorkersCapacity = workersCapacity;
        }
    }
}
