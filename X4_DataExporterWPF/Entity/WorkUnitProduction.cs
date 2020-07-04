namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 従業員用生産情報
    /// </summary>
    public class WorkUnitProduction
    {
        public string WorkUnitID { get; }

        public int Time { get; }

        public int Amount { get; }

        public string Method { get; }

        public WorkUnitProduction(string workUnitID, int time, int amount, string method)
        {
            this.WorkUnitID = workUnitID;
            this.Time = time;
            this.Amount = amount;
            this.Method = method;
        }
    }
}
