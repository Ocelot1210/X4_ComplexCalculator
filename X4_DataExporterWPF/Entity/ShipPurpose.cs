namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船用途
    /// </summary>
    public class ShipPurpose
    {
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ShipID { get; }


        /// <summary>
        /// 用途区分(primary等)
        /// </summary>
        public string Type { get; }


        /// <summary>
        /// 用途ID
        /// </summary>
        public string PurposeID { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <param name="type">用途区分(primary等)</param>
        /// <param name="purposeID">用途ID</param>
        public ShipPurpose(string shipID, string type, string purposeID)
        {
            ShipID = shipID;
            Type = type;
            PurposeID = purposeID;
        }
    }
}
