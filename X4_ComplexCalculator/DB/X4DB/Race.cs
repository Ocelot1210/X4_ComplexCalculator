﻿using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 種族管理用クラス
    /// </summary>
    class Race
    {
        /// <summary>
        /// 種族ID
        /// </summary>
        public string RaceID { get; }


        /// <summary>
        /// 種族名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="raceID">種族ID</param>
        public Race(string raceID)
        {
            RaceID = raceID;
            string name = "";
            DBConnection.X4DB.ExecQuery($"SELECT Name FROM Race WHERE RaceID = '{RaceID}'", (SQLiteDataReader dr, object[] args) => { name = dr["Name"].ToString(); });
            Name = name;
        }

        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is Race tgt && tgt.RaceID == RaceID;
        }

        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode()
        {
            return RaceID.GetHashCode();
        }
    }
}
