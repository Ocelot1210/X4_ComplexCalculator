namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船のカーゴタイプ
    /// </summary>
    public class ShipTransportType
    {
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ShipID { get; }


        /// <summary>
        /// カーゴ種別
        /// </summary>
        public string TransportTypeID { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <param name="transportTypeID">カーゴ種別</param>
        public ShipTransportType(string shipID, string transportTypeID)
        {
            ShipID = shipID;
            TransportTypeID = transportTypeID;
        }
    }
}
