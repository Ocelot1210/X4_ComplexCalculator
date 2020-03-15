using System;
using System.Collections.Generic;
using System.Text;

namespace X4_ComplexCalculator.Main.ProductsGrid
{
    /// <summary>
    /// 製造モジュール情報
    /// </summary>
    class ProductionModuleInfo
    {
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }

        /// <summary>
        /// 従業員数
        /// </summary>
        public long MaxWorkers { get; }

        /// <summary>
        /// 収容人数
        /// </summary>
        public long WorkersCapacity { get; }

        /// <summary>
        /// モジュール種別ID
        /// </summary>
        public string ModuleTypeID { get; }

        /// <summary>
        /// モジュール数
        /// </summary>
        public long Count { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="maxWorkers">従業員数</param>
        /// <param name="workersCapacity">収容人数</param>
        /// <param name="moduleTypeID">モジュール種別ID</param>
        /// <param name="count">モジュール数</param>
        public ProductionModuleInfo(string moduleID, long maxWorkers, long workersCapacity, string moduleTypeID, long count)
        {
            ModuleID = moduleID;
            MaxWorkers = maxWorkers;
            WorkersCapacity = workersCapacity;
            ModuleTypeID = moduleTypeID;
            Count = count;
        }
    }
}
