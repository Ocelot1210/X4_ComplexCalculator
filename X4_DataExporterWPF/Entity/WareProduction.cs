namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア生産時の情報
    /// </summary>
    public class WareProduction
    {
        #region プロパティ
        /// <summary>
        /// ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// ウェア生産方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// ウェア生産方式名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 生産量
        /// </summary>
        public long Amount { get; }


        /// <summary>
        /// 生産にかかる時間
        /// </summary>
        public double Time { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">ウェア生産方式</param>
        /// <param name="name">ウェア生産方式名</param>
        /// <param name="amount">生産量</param>
        /// <param name="time">生産にかかる時間</param>
        public WareProduction(string wareID, string method, string name, long amount, double time)
        {
            WareID = wareID;
            Method = method;
            Name = name;
            Amount = amount;
            Time = time;
        }
    }
}
