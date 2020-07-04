namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール
    /// </summary>
    public class Module
    {
        public string ModuleID { get; }

        public string ModuleTypeID { get; }

        public string Name { get; }

        public string Macro { get; }

        public int MaxWorkers { get; }

        public int WorkersCapacity { get; }

        public Module(
            string moduleID, string moduleTypeID, string name,
            string macro, int maxWorkers, int workersCapacity
        )
        {
            this.ModuleID = moduleID;
            this.ModuleTypeID = moduleTypeID;
            this.Name = name;
            this.Macro = macro;
            this.MaxWorkers = maxWorkers;
            this.WorkersCapacity = workersCapacity;
        }

    }
}
