using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.StationSummary.WorkForce
{
    /// <summary>
    /// 労働力用ListVierのアイテム
    /// </summary>
    class WorkForceDetailsItem : INotifyPropertyChangedBace
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
            get
            {
                return _ModuleCount;
            }
            set
            {
                if (_ModuleCount != value)
                {
                    _ModuleCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged("TotalWorkforce");
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
        public WorkForceDetailsItem(Module module, long moduleCount)
        {
            ModuleID = module.ModuleID;
            ModuleName = module.Name;
            _ModuleCount = moduleCount;
            MaxWorkers = module.MaxWorkers;
            WorkersCapacity = module.WorkersCapacity;
        }
    }
}
