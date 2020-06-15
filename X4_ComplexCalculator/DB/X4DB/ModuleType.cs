using System;
using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュール種別管理用クラス
    /// </summary>
    public class ModuleType
    {
        #region スタティックメンバ
        /// <summary>
        /// モジュール種別一覧
        /// </summary>
        static readonly Dictionary<string, ModuleType> _ModuleTypes = new Dictionary<string, ModuleType>();
        #endregion

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
        private ModuleType(string moduleTypeID, string name)
        {
            ModuleTypeID = moduleTypeID;
            Name = name;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _ModuleTypes.Clear();
            DBConnection.X4DB.ExecQuery($"SELECT ModuleTypeID, Name FROM ModuleType", (dr, args) =>
            {
                var id = (string)dr["ModuleTypeID"];
                var name = (string)dr["Name"];

                _ModuleTypes.Add(id, new ModuleType(id, name));
            });
        }


        /// <summary>
        /// モジュール種別IDに対応するモジュール種別を取得
        /// </summary>
        /// <param name="moduleTypeID">ジュール種別ID</param>
        /// <returns>モジュール種別</returns>
        public static ModuleType Get(string moduleTypeID) => _ModuleTypes[moduleTypeID];


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
