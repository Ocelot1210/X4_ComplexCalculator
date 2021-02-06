namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア生産に必要なウェア情報
    /// </summary>
    public class WareResource
    {
        #region プロパティ
        /// <summary>
        /// 生産対象ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// 生産方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 生産に必要なウェアID
        /// </summary>
        public string NeedWareID { get; }


        /// <summary>
        /// 生産に必要なウェア数
        /// </summary>
        public long Amount { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">生産対象ウェアID</param>
        /// <param name="method">生産方式</param>
        /// <param name="needWareID">生産に必要なウェアID</param>
        /// <param name="amount">生産に必要なウェア数</param>
        public WareResource(string wareID, string method, string needWareID, long amount)
        {
            WareID = wareID;
            Method = method;
            NeedWareID = needWareID;
            Amount = amount;
        }
    }
}
