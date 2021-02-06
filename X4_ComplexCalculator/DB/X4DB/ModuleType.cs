using System;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュール種別管理用クラス
    /// </summary>
    public class ModuleType
    {
        #region プロパティ
        /// <summary>
        /// モジュール種別ID
        /// </summary>
        public string ModuleTypeID { get; }


        /// <summary>
        /// モジュール種別名
        /// </summary>
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
        public override bool Equals(object? obj) => obj is ModuleType tgt && ModuleTypeID == tgt.ModuleTypeID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(ModuleTypeID);
    }
}
