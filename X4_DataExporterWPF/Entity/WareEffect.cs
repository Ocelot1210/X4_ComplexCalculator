using System;
using System.Collections.Generic;

namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア生産時の追加効果
    /// </summary>
    public class WareEffect : IEquatable<WareEffect>, IEqualityComparer<WareEffect>
    {
        #region プロパティ
        /// <summary>
        /// ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// ウェア生産方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 追加効果ID
        /// </summary>
        public string EffectID { get; }


        /// <summary>
        /// 生産倍率
        /// </summary>
        public double Product { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">ウェア生産方式</param>
        /// <param name="effectID">追加効果ID</param>
        /// <param name="product">生産倍率</param>
        public WareEffect(string wareID, string method, string effectID, double product)
        {
            WareID = wareID;
            Method = method;
            EffectID = effectID;
            Product = product;
        }


        /// <summary>
        /// 指定のオブジェクトと等価であるかを判定する
        /// </summary>
        /// <param name="effect">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public bool Equals(WareEffect? effect)
            => this.WareID == effect?.WareID && this.Method == effect.Method
            && this.EffectID == effect.EffectID && this.Product == effect.Product;


        /// <summary>
        /// 指定した 2 つのオブジェクトが等価であるかを判定する
        /// </summary>
        /// <param name="x">比較対象のオブジェクト</param>
        /// <param name="y">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public bool Equals(WareEffect? x, WareEffect? y) => x?.Equals(y) ?? false;


        /// <summary>
        /// 指定したオブジェクトのハッシュコードを算出する
        /// </summary>
        /// <param name="obj">算出対象のオブジェクト</param>
        /// <returns>指定したオブジェクトのハッシュコード</returns>
        public int GetHashCode(WareEffect obj) => obj.GetHashCode();


        /// <summary>
        /// 指定のオブジェクトと等価であるかを判定する
        /// </summary>
        /// <param name="obj">比較対象のオブジェクト</param>
        /// <returns>等価である場合は true、それ以外の場合は false</returns>
        public override bool Equals(object? obj) => obj is WareEffect effect && Equals(effect);


        /// <summary>
        /// 指定したオブジェクトのハッシュコードを算出する
        /// </summary>
        /// <returns>指定したオブジェクトのハッシュコード</returns>
        public override int GetHashCode() => HashCode.Combine(this.WareID, this.Method, this.EffectID, this.Product);
    }
}
