using System;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 派閥管理用クラス
    /// </summary>
    class Faction : IComparable
    {
        #region プロパティ
        /// <summary>
        /// 派閥ID
        /// </summary>
        public string FactionID { get; }


        /// <summary>
        /// 派閥名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 種族
        /// </summary>
        public Race Race { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="factionID">派閥ID</param>
        public Faction(string factionID)
        {
            FactionID = factionID;
            string name = "";
            Race race = null;

            DBConnection.X4DB.ExecQuery(
                $"SELECT Name, RaceID FROM Faction WHERE FactionID = '{FactionID}'", 
                (SQLiteDataReader dr, object[] args) => 
                {
                    name = dr["Name"].ToString();
                    race = new Race(dr["RaceID"].ToString());
                });
            Name = name;
            Race = race;
        }


        /// <summary>
        /// オブジェクト比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            return FactionID.CompareTo(obj is Faction);
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is Faction tgt && tgt.FactionID == FactionID;
        }


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode()
        {
            return FactionID.GetHashCode();
        }
    }
}
