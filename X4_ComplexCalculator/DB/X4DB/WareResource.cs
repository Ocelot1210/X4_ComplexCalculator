namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 1サイクルのウェア生産に必要なウェア情報
    /// </summary>
    public class WareResource
    {
        #region プロパティ
        /// <summary>
        /// ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// 生産方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 必要ウェアID
        /// </summary>
        public string NeedWareID { get; }


        /// <summary>
        /// 必要量
        /// </summary>
        public long Amount { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">生産方式</param>
        /// <param name="needWareID">必要ウェアID</param>
        /// <param name="amount">必要量</param>
        public WareResource(string wareID, string method, string needWareID, long amount)
        {
            WareID = wareID;
            Method = method;
            NeedWareID = needWareID;
            Amount = amount;
        }
    }
}
