using System;
using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 派閥管理用クラス
    /// </summary>
    public class Faction
    {
        #region スタティックメンバ
        /// <summary>
        /// 派閥一覧
        /// </summary>
        private readonly static Dictionary<string, Faction> _Factions = new Dictionary<string, Faction>();
        #endregion

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
        /// 派閥の種族
        /// </summary>
        public Race Race { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="factionID">派閥ID</param>
        /// <param name="name">派閥名</param>
        /// <param name="race">種族</param>
        private Faction(string factionID, string name, Race race)
        {
            FactionID = factionID;
            Name = name;
            Race = race;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _Factions.Clear();
            DBConnection.X4DB.ExecQuery("SELECT FactionID, Name, RaceID FROM Faction", (dr, args) =>
            {
                var id = (string)dr["FactionID"];
                var name = (string)dr["Name"];
                var raceID = (string)dr["RaceID"];

                _Factions.Add(id, new Faction(id, name, Race.Get(raceID)));
            });
        }

        /// <summary>
        /// 派閥IDに対応する派閥を取得
        /// </summary>
        /// <param name="factionID">派閥ID</param>
        /// <returns>派閥</returns>
        public static Faction? Get(string factionID)
            => _Factions.TryGetValue(factionID, out var faction) ? faction : null;


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is Faction tgt && tgt.FactionID == FactionID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(FactionID);
    }
}
