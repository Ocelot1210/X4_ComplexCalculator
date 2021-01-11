namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船製造に必要なウェア情報
    /// </summary>
    public class ShipResource
    {
        #region メンバ
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ShipID { get; }


        /// <summary>
        /// 艦船建造方法
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 必要ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// 必要ウェア数
        /// </summary>
        public int Amount { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shipID">装備ID</param>
        /// <param name="method">装備作成方法</param>
        /// <param name="needWareID">必要ウェアID</param>
        /// <param name="amount">必要ウェア数</param>
        public ShipResource(string shipID, string method, string needWareID, int amount)
        {
            ShipID = shipID;
            Method = method;
            WareID = needWareID;
            Amount = amount;
        }

    }
}
