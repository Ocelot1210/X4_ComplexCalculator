namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 装備種別
    /// </summary>
    public class EquipmentType
    {
        public string EquipmentTypeID { get; }

        public string Name { get; }

        public EquipmentType(string equipmentTypeID, string name)
        {
            this.EquipmentTypeID = equipmentTypeID;
            this.Name = name;
        }
    }
}
