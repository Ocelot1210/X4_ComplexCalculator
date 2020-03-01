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
        public string ModuleTypeID { private set; get; }


        /// <summary>
        /// モジュール種別名
        /// </summary>
        public string Name { private set; get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">モジュール種別ID</param>
        public ModuleType(string id)
        {
            ModuleTypeID = id;
            DBConnection.X4DB.ExecQuery($"SELECT Name FROM ModuleType WHERE ModuleTypeID = '{id}'", (SQLiteDataReader dr, object[] args) => { Name = dr["Name"].ToString(); });
        }

        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
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
