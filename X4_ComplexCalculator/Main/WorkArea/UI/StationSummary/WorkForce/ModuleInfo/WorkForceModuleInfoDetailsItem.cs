using Prism.Mvvm;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.ModuleInfo
{
    /// <summary>
    /// 労働力用ListVierのアイテム
    /// </summary>
    class WorkForceModuleInfoDetailsItem : BindableBase
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
        public WorkForceModuleInfoDetailsItem(Module module, long moduleCount)
        {
            ModuleID = module.ModuleID;
            ModuleName = module.Name;
            _ModuleCount = moduleCount;
            MaxWorkers = module.MaxWorkers;
            WorkersCapacity = module.WorkersCapacity;
        }
    }
}
