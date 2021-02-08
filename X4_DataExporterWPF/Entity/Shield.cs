namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// シールド情報
    /// </summary>
    public class Shield
    {
        #region プロパティ
        /// <summary>
        /// 装備ID
        /// </summary>
        public string EquipmentID { get; }


        /// <summary>
        /// 最大シールド容量
        /// </summary>
        public long Capacity { get; }


        /// <summary>
        /// 再充電率
        /// </summary>
        public long RechargeRate { get; }


        /// <summary>
        /// 再充電遅延
        /// </summary>
        public double RechargeDelay { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <param name="hull">船体強度</param>
        /// <param name="capacity">最大シールド容量</param>
        /// <param name="rechargeRate">再充電率</param>
        /// <param name="rechargeDelay">再充電遅延</param>
        public Shield(string equipmentID, long capacity, long rechargeRate, double rechargeDelay)
        {
            EquipmentID = equipmentID;
            Capacity = capacity;
            RechargeRate = rechargeRate;
            RechargeDelay = rechargeDelay;
        }
    }
}
