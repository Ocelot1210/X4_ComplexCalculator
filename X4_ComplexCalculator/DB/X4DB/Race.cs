using System;
using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 種族管理用クラス
    /// </summary>
    public class Race
    {
        #region スタティックメンバ
        /// <summary>
        /// 種族一覧
        /// </summary>
        private readonly static Dictionary<string, Race> _Races = new Dictionary<string, Race>();
        #endregion

        #region プロパティ
        /// <summary>
        /// 種族ID
        /// </summary>
        public string RaceID { get; }


        /// <summary>
        /// 種族名
        /// </summary>
        public string Name { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="raceID">種族ID</param>
        /// <param name="name">種族名</param>
        private Race(string raceID, string name)
        {
            RaceID = raceID;
            Name = name;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _Races.Clear();
            DBConnection.X4DB.ExecQuery("SELECT RaceID, Name FROM Race", (dr, args) =>
            {
                var id = (string)dr["RaceID"];
                var name = (string)dr["Name"];

                _Races.Add(id, new Race(id, name));
            });
        }


        /// <summary>
        /// 種族を取得
        /// </summary>
        /// <param name="raceID">種族ID</param>
        /// <returns>種族IDに対応する種族</returns>
        public static Race? Get(string raceID) =>
            _Races.TryGetValue(raceID, out var race) ? race : null;


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is Race tgt && tgt.RaceID == RaceID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(RaceID);
    }
}
