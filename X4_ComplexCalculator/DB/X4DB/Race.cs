using System;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 種族管理用クラス
    /// </summary>
    public class Race
    {
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
        public Race(string raceID, string name, string shortName, string description)
        {
            RaceID = raceID;
            Name = name;
            ShortName = shortName;
            Description = description;
        }


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
