namespace X4_ComplexCalculator.DB.X4DB.Interfaces
{
    /// <summary>
    /// 1サイクルのウェア生産に必要なウェア情報用インターフェイス
    /// </summary>
    public interface IWareResource
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
        /// 必要ウェアID
        /// </summary>
        public string NeedWareID { get; }


        /// <summary>
        /// 必要量
        /// </summary>
        public long Amount { get; }
        #endregion
    }
}
