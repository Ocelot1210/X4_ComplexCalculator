using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.StationSummary
{
    /// <summary>
    /// 労働力用ListVierのアイテム
    /// </summary>
    class WorkForceDetailsItem
    {
        /// <summary>
        /// モジュール名
        /// </summary>
        public string ModuleName { get; private set; }


        /// <summary>
        /// モジュール数
        /// </summary>
        public long ModuleCount { get; private set; }


        /// <summary>
        /// 必要労働力
        /// </summary>
        public long MaxWorkers { get; private set; } = 0;


        /// <summary>
        /// 収容人数
        /// </summary>
        public long WorkersCapacity { get; private set; } = 0;


        /// <summary>
        /// 労働者数
        /// </summary>
        public long WorkForce => WorkersCapacity - MaxWorkers;


        /// <summary>
        /// 総労働者数
        /// </summary>
        public long TotalWorkforce => ModuleCount * WorkForce;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">モジュールオブジェクト</param>
        /// <param name="moduleCount">モジュール数</param>
        public WorkForceDetailsItem(Module module, long moduleCount)
        {
            ModuleName = module.Name;
            ModuleCount = moduleCount;
            MaxWorkers = module.MaxWorkers;
            WorkersCapacity = module.WorkersCapacity;
        }
    }
}
