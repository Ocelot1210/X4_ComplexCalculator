namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 装備作成時の情報
    /// </summary>
    public class EquipmentProduction
    {
        public string EquipmentID { get; }

        public string Method { get; }

        public double Time { get; }

        public EquipmentProduction(string equipmentID, string method, double time)
        {
            this.EquipmentID = equipmentID;
            this.Method = method;
            this.Time = time;
        }
    }
}
