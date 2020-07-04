namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 装備生産に必要なウェア情報
    /// </summary>
    public class EquipmentResource
    {
        public string EquipmentID { get; }

        public string Method { get; }

        public string NeedWareID { get; }

        public int Amount { get; }

        public EquipmentResource(string equipmentID, string method, string needWareID, int amount)
        {
            this.EquipmentID = equipmentID;
            this.Method = method;
            this.NeedWareID = needWareID;
            this.Amount = amount;
        }

    }
}
