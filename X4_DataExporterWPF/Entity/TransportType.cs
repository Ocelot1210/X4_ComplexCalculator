namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// カーゴ種別
    /// </summary>
    public class TransportType
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


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="transportTypeID">カーゴ種別ID</param>
        /// <param name="name">カーゴ種別名</param>
        public TransportType(string transportTypeID, string name)
        {
            TransportTypeID = transportTypeID;
            Name = name;
        }
    }
}
