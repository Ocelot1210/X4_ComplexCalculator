using System;
using System.Drawing;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// 派閥情報用クラス
    /// </summary>
    public class Faction : IFaction
    {
        #region IFaction
        /// <inheritdoc/>
        public string FactionID { get; }


        /// <inheritdoc/>
        public string Name { get; }


        /// <inheritdoc/>
        public IRace Race { get; }


        /// <inheritdoc/>
        public Color Color { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="factionID">派閥ID</param>
        /// <param name="name">派閥名</param>
        /// <param name="race">種族</param>
        public Faction(string factionID, string name, IRace race, int color)
        {
            FactionID = factionID;
            Name = name;
            Race = race;
            Color = Color.FromArgb(color);
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is IFaction other && other.FactionID == FactionID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(FactionID);
    }
}
