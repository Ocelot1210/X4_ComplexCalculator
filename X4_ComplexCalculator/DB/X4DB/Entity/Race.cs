using System;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// 種族情報用クラス
    /// </summary>
    public class Race : IRace
    {
        #region IRace
        /// <inheritdoc/>
        public string RaceID { get; }


        /// <inheritdoc/>
        public string Name { get; }


        /// <inheritdoc/>
        public string ShortName { get; }


        /// <inheritdoc/>
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
        public override bool Equals(object? obj) => obj is IRace other && other.RaceID == RaceID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(RaceID);
    }
}
