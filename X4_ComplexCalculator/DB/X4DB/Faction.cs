using System;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 派閥管理用クラス
    /// </summary>
    public class Faction
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
        public Faction(string factionID, string name, Race race)
        {
            FactionID = factionID;
            Name = name;
            Race = race;
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is Faction other && other.FactionID == FactionID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(FactionID);
    }
}
