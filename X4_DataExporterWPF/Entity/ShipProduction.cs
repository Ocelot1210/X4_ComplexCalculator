namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船作成時の情報
    /// </summary>
    public class ShipProduction
    {
        #region プロパティ
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ShipID { get; }


        /// <summary>
        /// 艦船建造方法
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 艦船建造にかかる時間
        /// </summary>
        public double Time { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <param name="method">艦船建造方法</param>
        /// <param name="time">艦船建造にかかる時間</param>
        public ShipProduction(string shipID, string method, double time)
        {
            ShipID = shipID;
            Method = method;
            Time = time;
        }
    }
}
