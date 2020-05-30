using System;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュール種別管理用クラス
    /// </summary>
    public class ModuleType
    {
        /// <summary>
        /// モジュール種別ID
        /// </summary>
        public string ModuleTypeID { get; }


        /// <summary>
        /// モジュール種別名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleTypeID">モジュール種別ID</param>
        public ModuleType(string moduleTypeID)
        {
            ModuleTypeID = moduleTypeID;
            string name = "";
            DBConnection.X4DB.ExecQuery($"SELECT Name FROM ModuleType WHERE ModuleTypeID = '{moduleTypeID}'", (SQLiteDataReader dr, object[] args) => { name = (string)dr["Name"]; });

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid module type id.", nameof(moduleTypeID));
            }

            Name = name;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="moduleType"></param>
        public ModuleType(ModuleType moduleType)
        {
            ModuleTypeID = moduleType.ModuleTypeID;
            Name = moduleType.Name;
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (!(obj is ModuleType tgt)) return false;

            return ModuleTypeID == tgt.ModuleTypeID;
        }


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode()
        {
            return ModuleTypeID.GetHashCode();
        }
    }
}
