namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュールのタレット
    /// </summary>
    public class ModuleTurret
    {
        public string ModuleID { get; }

        public string SizeID { get; }

        public int Amount { get; }

        public ModuleTurret(string moduleID, string sizeID, int amount)
        {
            this.ModuleID = moduleID;
            this.SizeID = sizeID;
            this.Amount = amount;
        }
    }
}
