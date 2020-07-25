namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 従業員用生産情報
    /// </summary>
    public class WorkUnitProduction
    {
        #region プロパティ
        /// <summary>
        /// 労働種別ID
        /// </summary>
        public string WorkUnitID { get; }


        /// <summary>
        /// 労働時間
        /// </summary>
        public int Time { get; }


        /// <summary>
        /// 労働者数(計算用基準値)
        /// </summary>
        public int Amount { get; }


        /// <summary>
        /// 労働方式
        /// </summary>
        public string Method { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="workUnitID">労働種別ID</param>
        /// <param name="time">労働時間</param>
        /// <param name="amount">労働時間に対して必要なウェア数量</param>
        /// <param name="method">労働方式</param>
        public WorkUnitProduction(string workUnitID, int time, int amount, string method)
        {
            WorkUnitID = workUnitID;
            Time = time;
            Amount = amount;
            Method = method;
        }
    }
}
