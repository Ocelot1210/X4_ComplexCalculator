namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 装備
    /// </summary>
    public class Equipment
    {
        public string EquipmentID { get; }

        public string MacroName { get; }

        public string EquipmentTypeID { get; }

        public string SizeID { get; }

        public string Name { get; }

        public Equipment(string equipmentID, string macroName, string equipmentTypeID, string sizeID, string name)
        {
            this.EquipmentID = equipmentID;
            this.MacroName = macroName;
            this.EquipmentTypeID = equipmentTypeID;
            this.SizeID = sizeID;
            this.Name = name;
        }
    }
}
