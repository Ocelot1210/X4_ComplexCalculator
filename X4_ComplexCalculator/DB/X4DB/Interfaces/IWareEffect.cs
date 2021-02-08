namespace X4_ComplexCalculator.DB.X4DB.Interfaces
{
    /// <summary>
    /// ウェア単位のウェア生産時の追加効果情報用インターフェイス
    /// </summary>
    public interface IWareEffect
    {
        #region プロパティ
        /// <summary>
        /// ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// 生産方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 追加効果ID
        /// </summary>
        public string EffectID { get; }


        /// <summary>
        /// 追加効果の値
        /// </summary>
        public double Product { get; }
        #endregion
    }
}
