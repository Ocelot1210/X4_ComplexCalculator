namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 従業員が必要とするウェア情報
    /// </summary>
    public class WorkUnitResource
    {
        public string WorkUnitID { get; }

        public string Method { get; }

        public string WareID { get; }

        public int Amount { get; }

        public WorkUnitResource(string workUnitID, string method, string wareID, int amount)
        {
            this.WorkUnitID = workUnitID;
            this.Method = method;
            this.WareID = wareID;
            this.Amount = amount;
        }
    }
}
