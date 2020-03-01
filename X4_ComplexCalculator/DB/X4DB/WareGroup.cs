﻿using System.Data.SQLite;

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
        /// <param name="id">ウェアグループID</param>
        public WareGroup(string id)
        {
            WareGroupID = id;
            DBConnection.X4DB.ExecQuery(
                $"SELECT Name, Tier FROM WareGroup WHERE WareGroupID = '{WareGroupID}'",
                (SQLiteDataReader dr, object[] args) =>
                {
                    Name = dr["Name"].ToString();
                    Tier = (long)dr["Tier"];
                });
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
