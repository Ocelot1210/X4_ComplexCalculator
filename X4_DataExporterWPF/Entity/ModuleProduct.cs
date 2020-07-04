namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュールの生産品
    /// </summary>
    public class ModuleProduct
    {
        public string ModuleID { get; }

        public string WareID { get; }

        public string Method { get; }

        public ModuleProduct(string moduleID, string wareID, string method)
        {
            this.ModuleID = moduleID;
            this.WareID = wareID;
            this.Method = method;
        }
    }
}
