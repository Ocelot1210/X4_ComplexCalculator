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
            Race? race = null;

            DBConnection.X4DB.ExecQuery(
                $"SELECT Name, RaceID FROM Faction WHERE FactionID = '{FactionID}'", 
                (SQLiteDataReader dr, object[] args) => 
                {
                    name = (string)dr["Name"];
                    race = new Race((string)dr["RaceID"]);
                });

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid faction id.", nameof(factionID));
            }

            Name = name;
            Race = race ?? throw new InvalidOperationException();
        }


        /// <summary>
        /// オブジェクト比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }
            return FactionID.CompareTo(obj is Faction);
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                throw new ArgumentException($"parameter {nameof(obj)} should not be null.", nameof(obj));
            }
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
