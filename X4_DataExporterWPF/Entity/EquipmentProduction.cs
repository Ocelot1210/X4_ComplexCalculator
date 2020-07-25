namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 装備作成時の情報
    /// </summary>
    public class EquipmentProduction
    {
        #region プロパティ
        /// <summary>
        /// 装備ID
        /// </summary>
        public string EquipmentID { get; }


        /// <summary>
        /// 装備作成方法
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 装備作成にかかる時間
        /// </summary>
        public double Time { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <param name="method">装備作成方法</param>
        /// <param name="time">装備作成にかかる時間</param>
        public EquipmentProduction(string equipmentID, string method, double time)
        {
            EquipmentID = equipmentID;
            Method = method;
            Time = time;
        }
    }
}
