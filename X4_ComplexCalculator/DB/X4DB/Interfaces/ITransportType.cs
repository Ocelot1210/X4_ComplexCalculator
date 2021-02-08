namespace X4_ComplexCalculator.DB.X4DB.Interfaces
{
    /// <summary>
    /// カーゴタイプ(輸送種別)情報用インターフェイス
    /// </summary>
    public interface ITransportType
    {
        #region プロパティ
        /// <summary>
        /// カーゴ種別ID
        /// </summary>
        public string TransportTypeID { get; }


        /// <summary>
        /// カーゴ種別名
        /// </summary>
        public string Name { get; }
        #endregion
    }
}
