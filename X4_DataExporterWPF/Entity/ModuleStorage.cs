namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュールの保管容量
    /// </summary>
    public class ModuleStorage
    {
        public string ModuleID { get; }

        public string TransportTypeID { get; }

        public int Amount { get; }

        public ModuleStorage(string moduleID, string transportTypeID, int amount)
        {
            this.ModuleID = moduleID;
            this.TransportTypeID = transportTypeID;
            this.Amount = amount;
        }
    }
}
