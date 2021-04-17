using System;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// モジュール種別情報用クラス
    /// </summary>
    public class ModuleType : IModuleType
    {
        #region IModuleType
        /// <inheritdoc/>
        public string ModuleTypeID { get; }


        /// <inheritdoc/>
        public string Name { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleTypeID">モジュール種別ID</param>
        /// <param name="name">モジュール種別名</param>
        public ModuleType(string moduleTypeID, string name)
        {
            ModuleTypeID = moduleTypeID;
            Name = name;
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is IModuleType other && ModuleTypeID == other.ModuleTypeID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(ModuleTypeID);
    }
}
