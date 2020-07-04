namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール建造に関する情報
    /// </summary>
    public class ModuleProduction
    {
        public string ModuleID { get; }

        public string Method { get; }

        public double Time { get; }

        public ModuleProduction(string moduleID, string method, double time)
        {
            this.ModuleID = moduleID;
            this.Method = method;
            this.Time = time;
        }
    }
}
