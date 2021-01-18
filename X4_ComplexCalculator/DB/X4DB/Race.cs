using System;
using System.Linq;
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
        private readonly static Dictionary<string, Race> _Races = new();
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


        /// <summary>
        /// 略称
        /// </summary>
        public string ShortName { get; }


        /// <summary>
        /// 説明文
        /// </summary>
        public string Description { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="raceID">種族ID</param>
        /// <param name="name">種族名</param>
        /// <param name="shortName">略称</param>
        /// <param name="description">説明文</param>
        private Race(string raceID, string name, string shortName, string description)
        {
            RaceID = raceID;
            Name = name;
            ShortName = shortName;
            Description = description;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _Races.Clear();

            const string sql = "SELECT RaceID, Name, ShortName, Description FROM Race";
            foreach (var item in X4Database.Instance.Query<Race>(sql))
            {
                _Races.Add(item.RaceID, item);
            }
        }


        /// <summary>
        /// 種族を取得
        /// </summary>
        /// <param name="raceID">種族ID</param>
        /// <returns>種族IDに対応する種族</returns>
        public static Race? Get(string raceID) =>
            _Races.TryGetValue(raceID, out var race) ? race : null;


        /// <summary>
        /// 全種族を取得
        /// </summary>
        public static IEnumerable<Race> GetAll() => _Races.Values;


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
