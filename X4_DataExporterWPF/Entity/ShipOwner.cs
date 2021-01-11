namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船所有派閥
    /// </summary>
    public class ShipOwner
    {
        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ShipID { get; }


        /// <summary>
        /// 派閥ID
        /// </summary>
        public string FactionID { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <param name="factionID">派閥ID</param>
        public ShipOwner(string shipID, string factionID)
        {
            ShipID = shipID;
            FactionID = factionID;
        }
    }
}
