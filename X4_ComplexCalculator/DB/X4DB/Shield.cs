namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// シールド情報
    /// </summary>
    public class Shield : Equipment
    {
        #region プロパティ
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
        /// <param name="id">装備ID</param>
        internal Shield(string id) : base(id)
        {
            const string sql = "SELECT Capacity RechargeRate, RechargeDelay FROM Shield WHERE EquipmentID = :EquipmentID";

            (
                Capacity,
                RechargeRate,
                RechargeDelay
            ) = X4Database.Instance.QuerySingle<(long, long, double)>(sql, new { EquipmentID = id });
        }
    }
}
