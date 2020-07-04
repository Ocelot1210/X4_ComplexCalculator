namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール建造に必要なウェア情報
    /// </summary>
    public class ModuleResource
    {
        public string ModuleID { get; }

        public string Method { get; }

        public string WareID { get; }

        public int Amount { get; }

        public ModuleResource(string moduleID, string method, string wareID, int amount)
        {
            this.ModuleID = moduleID;
            this.Method = method;
            this.WareID = wareID;
            this.Amount = amount;
        }
    }
}
