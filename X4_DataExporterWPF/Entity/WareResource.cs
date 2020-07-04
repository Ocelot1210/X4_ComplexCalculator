namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア生産に必要な情報
    /// </summary>
    public class WareResource
    {
        public string WareID { get; }

        public string Method { get; }

        public string NeedWareID { get; }

        public int Amount { get; }

        public WareResource(string wareID, string method, string needWareID, int amount)
        {
            this.WareID = wareID;
            this.Method = method;
            this.NeedWareID = needWareID;
            this.Amount = amount;
        }
    }
}
