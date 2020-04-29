using System;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// ウェア種別(グループ)管理用クラス
    /// </summary>
    public class WareGroup
    {
        #region プロパティ
        /// <summary>
        /// ウェアグループID
        /// </summary>
        public string WareGroupID { private set; get; }


        /// <summary>
        /// ウェアグループ名
        /// </summary>
        public string Name { private set; get; }


        /// <summary>
        /// 階層
        /// </summary>
        public long Tier { private set; get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareGroupID">ウェアグループID</param>
        public WareGroup(string wareGroupID)
        {
            WareGroupID = wareGroupID;
            string name = "";
            long tier = 0;
            DBConnection.X4DB.ExecQuery($"SELECT Name, Tier FROM WareGroup WHERE WareGroupID = '{WareGroupID}'",(SQLiteDataReader dr, object[] args) =>
            {
                name = (string)dr["Name"];
                tier = (long)dr["Tier"];
            });

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid ware group id.", nameof(wareGroupID));
            }

            Name = name;
            Tier = tier;
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is WareGroup tgt && tgt.WareGroupID == WareGroupID;
        }

        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode()
        {
            return WareGroupID.GetHashCode();
        }
    }
}
