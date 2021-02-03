using System;
using System.Collections.Generic;

namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア種別
    /// </summary>
    public class WareGroup : IEquatable<WareGroup>, IEqualityComparer<WareGroup>
    {
        #region プロパティ
        /// <summary>
        /// ウェア種別ID
        /// </summary>
        public string WareGroupID { get; }


        /// <summary>
        /// ウェア種別名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 階級
        /// </summary>
        public int Tier { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareGroupID">ウェア種別ID</param>
        /// <param name="name">ウェア種別名</param>
        /// <param name="factoryName">工場名</param>
        /// <param name="icon">アイコン名</param>
        /// <param name="tier">階級</param>
        public WareGroup(string wareGroupID, string name, int tier)
        {
            WareGroupID = wareGroupID;
            Name = name;
            Tier = tier;
        }


        /// <summary>
        /// 指定のオブジェクトと等価であるかを判定する
        /// </summary>
        /// <param name="group">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public bool Equals(WareGroup? group)
            => WareGroupID == group?.WareGroupID && Name == group.Name
            && Tier == group.Tier;


        /// <summary>
        /// 指定した 2 つのオブジェクトが等価であるかを判定する
        /// </summary>
        /// <param name="x">比較対象のオブジェクト</param>
        /// <param name="y">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public bool Equals(WareGroup? x, WareGroup? y) => x?.Equals(y) ?? false;


        /// <summary>
        /// 指定したオブジェクトのハッシュコードを算出する
        /// </summary>
        /// <param name="obj">算出対象のオブジェクト</param>
        /// <returns>指定したオブジェクトのハッシュコード</returns>
        public int GetHashCode(WareGroup obj) => obj.GetHashCode();


        /// <summary>
        /// 指定のオブジェクトと等価であるかを判定する
        /// </summary>
        /// <param name="obj">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public override bool Equals(object? obj) => obj is WareGroup group && Equals(group);


        /// <summary>
        /// 指定したオブジェクトのハッシュコードを算出する
        /// </summary>
        /// <returns>指定したオブジェクトのハッシュコード</returns>
        public override int GetHashCode()
            => HashCode.Combine(WareGroupID, Name, Tier);
    }
}
