using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// ウェア生産時の追加効果情報用クラス
    /// </summary>
    public class WareEffect : IWareEffect
    {
        #region プロパティ
        /// <inheritdoc/>
        public string WareID { get; }


        /// <inheritdoc/>
        public string Method { get; }


        /// <inheritdoc/>
        public string EffectID { get; }


        /// <inheritdoc/>
        public double Product { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">生産方式</param>
        /// <param name="effectID">追加効果ID</param>
        /// <param name="product">追加効果の値</param>
        public WareEffect(string wareID, string method, string effectID, double product)
        {
            WareID = wareID;
            Method = method;
            EffectID = effectID;
            Product = product;
        }
    }
}
