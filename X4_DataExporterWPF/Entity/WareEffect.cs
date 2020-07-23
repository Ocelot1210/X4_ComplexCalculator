namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア生産時の追加効果
    /// </summary>
    public class WareEffect
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
        /// <param name="effectID">EffectID</param>
        /// <param name="product">生産倍率</param>
        public WareEffect(string wareID, string method, string effectID, double product)
        {
            WareID = wareID;
            Method = method;
            EffectID = effectID;
            Product = product;
        }
    }
}
