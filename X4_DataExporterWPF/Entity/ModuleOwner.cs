using System;
using System.Collections.Generic;

namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール所有派閥
    /// </summary>
    public class ModuleOwner : IEquatable<ModuleOwner>, IEqualityComparer<ModuleOwner>
    {
        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// 派閥ID
        /// </summary>
        public string FactionID { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="factionID">派閥ID</param>
        public ModuleOwner(string moduleID, string factionID)
        {
            ModuleID = moduleID;
            FactionID = factionID;
        }


        /// <summary>
        /// 指定のオブジェクトと等価であるかを判定する
        /// </summary>
        /// <param name="owner">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public bool Equals(ModuleOwner? owner)
            => this.ModuleID == owner?.ModuleID && this.FactionID == owner.FactionID;


        /// <summary>
        /// 指定した 2 つのオブジェクトが等価であるかを判定する
        /// </summary>
        /// <param name="x">比較対象のオブジェクト</param>
        /// <param name="y">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public bool Equals(ModuleOwner? x, ModuleOwner? y) => x?.Equals(y) ?? false;


        /// <summary>
        /// 指定したオブジェクトのハッシュコードを算出する
        /// </summary>
        /// <param name="obj">算出対象のオブジェクト</param>
        /// <returns>指定したオブジェクトのハッシュコード</returns>
        public int GetHashCode(ModuleOwner obj) => obj.GetHashCode();


        /// <summary>
        /// 指定のオブジェクトと等価であるかを判定する
        /// </summary>
        /// <param name="obj">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public override bool Equals(object? obj) => obj is ModuleOwner owner && Equals(owner);


        /// <summary>
        /// 指定したオブジェクトのハッシュコードを算出する
        /// </summary>
        /// <returns>指定したオブジェクトのハッシュコード</returns>
        public override int GetHashCode() => HashCode.Combine(this.ModuleID, this.FactionID);
    }
}
