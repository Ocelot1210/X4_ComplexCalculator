using System;
using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB.Interfaces
{
    public interface IX4Module : IEquippableWare, IMacro, IComparable
    {
        #region プロパティ
        /// <summary>
        /// モジュール種別
        /// </summary>
        public IModuleType ModuleType { get; }


        /// <summary>
        /// 従業員数
        /// </summary>
        public long MaxWorkers { get; }


        /// <summary>
        /// 最大収容人数
        /// </summary>
        public long WorkersCapacity { get; }


        /// <summary>
        /// 設計図が無いか
        /// </summary>
        public bool NoBluePrint { get; }


        /// <summary>
        /// モジュールの製品
        /// </summary>
        public IReadOnlyList<IModuleProduct> Products { get; }


        /// <summary>
        /// 保管庫情報
        /// </summary>
        public IModuleStorage Storage { get; }
        #endregion
    }
}
