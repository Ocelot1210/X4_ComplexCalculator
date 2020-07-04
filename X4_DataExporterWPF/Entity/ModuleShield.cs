namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュールのシールド
    /// </summary>
    public class ModuleShield
    {
        public string ModuleID { get; }

        public string SizeID { get; }

        public int Amount { get; }

        public ModuleShield(string moduleID, string sizeID, int amount)
        {
            this.ModuleID = moduleID;
            this.SizeID = sizeID;
            this.Amount = amount;
        }
    }
}
