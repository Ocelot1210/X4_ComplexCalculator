using System;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// ウェア種別(グループ)情報用クラス
    /// </summary>
    public class WareGroup : IWareGroup
    {
        #region プロパティ
        /// <inheritdoc/>
        public string WareGroupID { get; }


        /// <inheritdoc/>
        public string Name { get; }


        /// <inheritdoc/>
        public long Tier { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareGroupID">ウェアグループID</param>
        /// <param name="name">ウェアグループ名</param>
        /// <param name="Tier">階級</param>
        public WareGroup(string wareGroupID, string name, long tier)
        {
            WareGroupID = wareGroupID;
            Name = name;
            Tier = tier;
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is IWareGroup other && other.WareGroupID == WareGroupID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(WareGroupID);
    }
}
