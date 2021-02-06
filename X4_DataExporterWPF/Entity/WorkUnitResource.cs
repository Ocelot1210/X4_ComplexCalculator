namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 従業員が必要とするウェア情報
    /// </summary>
    public class WorkUnitResource
    {
        #region プロパティ
        /// <summary>
        /// 労働種別ID
        /// </summary>
        public string WorkUnitID { get; }


        /// <summary>
        /// 労働方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 労働に必要なウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// 労働に必要なウェア数量
        /// </summary>
        public long Amount { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="workUnitID"></param>
        /// <param name="method"></param>
        /// <param name="wareID"></param>
        /// <param name="amount"></param>
        public WorkUnitResource(string workUnitID, string method, string wareID, long amount)
        {
            WorkUnitID = workUnitID;
            Method = method;
            WareID = wareID;
            Amount = amount;
        }
    }
}
