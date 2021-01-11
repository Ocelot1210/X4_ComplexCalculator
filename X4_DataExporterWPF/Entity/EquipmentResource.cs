namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船製造に必要なウェア情報
    /// </summary>
    public class EquipmentResource
    {
        #region メンバ
        /// <summary>
        /// 装備ID
        /// </summary>
        public string EquipmentID { get; }


        /// <summary>
        /// 装備作成方法
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 必要ウェアID
        /// </summary>
        public string NeedWareID { get; }


        /// <summary>
        /// 必要ウェア数
        /// </summary>
        public int Amount { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <param name="method">装備作成方法</param>
        /// <param name="needWareID">必要ウェアID</param>
        /// <param name="amount">必要ウェア数</param>
        public EquipmentResource(string equipmentID, string method, string needWareID, int amount)
        {
            EquipmentID = equipmentID;
            Method = method;
            NeedWareID = needWareID;
            Amount = amount;
        }

    }
}
