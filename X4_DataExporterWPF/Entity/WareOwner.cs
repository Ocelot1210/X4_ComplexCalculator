﻿using System;
using System.Collections.Generic;

namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア所有派閥
    /// </summary>
    public class WareOwner : IEquatable<WareOwner>, IEqualityComparer<WareOwner>
    {
        #region プロパティ
        /// <summary>
        /// ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// 派閥ID
        /// </summary>
        public string FactionID { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="factionID">派閥ID</param>
        public WareOwner(string wareID, string factionID)
        {
            WareID = wareID;
            FactionID = factionID;
        }


        /// <summary>
        /// 指定のオブジェクトと等価であるかを判定する
        /// </summary>
        /// <param name="owner">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public bool Equals(WareOwner? owner)
            => this.WareID == owner?.WareID && this.FactionID == owner.FactionID;


        /// <summary>
        /// 指定した 2 つのオブジェクトが等価であるかを判定する
        /// </summary>
        /// <param name="x">比較対象のオブジェクト</param>
        /// <param name="y">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public bool Equals(WareOwner? x, WareOwner? y) => x?.Equals(y) ?? false;


        /// <summary>
        /// 指定したオブジェクトのハッシュコードを算出する
        /// </summary>
        /// <param name="obj">算出対象のオブジェクト</param>
        /// <returns>指定したオブジェクトのハッシュコード</returns>
        public int GetHashCode(WareOwner obj) => obj.GetHashCode();


        /// <summary>
        /// 指定のオブジェクトと等価であるかを判定する
        /// </summary>
        /// <param name="obj">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public override bool Equals(object? obj) => obj is WareOwner owner && Equals(owner);


        /// <summary>
        /// 指定したオブジェクトのハッシュコードを算出する
        /// </summary>
        /// <returns>指定したオブジェクトのハッシュコード</returns>
        public override int GetHashCode() => HashCode.Combine(this.WareID, this.FactionID);
    }
}
