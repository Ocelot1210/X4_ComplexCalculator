namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 装備保有派閥
    /// </summary>
    public class EquipmentOwner
    {
        #region プロパティ
        /// <summary>
        /// 装備ID
        /// </summary>
        public string EquipmentID { get; }


        /// <summary>
        /// 派閥ID
        /// </summary>
        public string FactionID { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <param name="factionID">派閥ID</param>
        public EquipmentOwner(string equipmentID, string factionID)
        {
            this.EquipmentID = equipmentID;
            this.FactionID = factionID;
        }
    }
}
