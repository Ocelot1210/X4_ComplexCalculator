namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア生産時の情報
    /// </summary>
    public class WareProduction
    {
        public string WareID { get; }

        public string Method { get; }

        public string Name { get; }

        public int Amount { get; }

        public double Time { get; }

        public WareProduction(string wareID, string method, string name, int amount, double time)
        {
            this.WareID = wareID;
            this.Method = method;
            this.Name = name;
            this.Amount = amount;
            this.Time = time;
        }
    }
}
