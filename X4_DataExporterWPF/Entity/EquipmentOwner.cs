namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 装備保有派閥
    /// </summary>
    public class EquipmentOwner
    {
        public string EquipmentID { get; }

        public string FactionID { get; }

        public EquipmentOwner(string equipmentID, string factionID)
        {
            this.EquipmentID = equipmentID;
            this.FactionID = factionID;
        }
    }
}
