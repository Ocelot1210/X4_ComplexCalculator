namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール種別
    /// </summary>
    public class ModuleType
    {
        public string ModuleTypeID { get; }

        public string Name { get; }

        public ModuleType(string moduleTypeID, string name)
        {
            this.ModuleTypeID = moduleTypeID;
            this.Name = name;
        }
    }
}
