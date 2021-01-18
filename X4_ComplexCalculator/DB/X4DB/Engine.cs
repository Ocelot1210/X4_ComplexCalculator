namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// エンジン情報
    /// </summary>
    public class Engine : Equipment
    {
        #region プロパティ
        /// <summary>
        /// 前方推進力
        /// </summary>
        public double ForwardThrust { get; }


        /// <summary>
        /// 後方推進力
        /// </summary>
        public double ReverseThrust { get; }


        /// <summary>
        /// ブースト推進力
        /// </summary>
        public double BoostThrust { get; }


        /// <summary>
        /// ブースト持続時間
        /// </summary>
        public double BoostDuration { get; }


        /// <summary>
        /// ブースト解除時間
        /// </summary>
        public double BoostReleaseTime { get; }


        /// <summary>
        /// トラベル推進力
        /// </summary>
        public double TravelThrust { get; }


        /// <summary>
        /// トラベル解除時間
        /// </summary>
        public double TravelReleaseTime { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">装備ID</param>
        internal Engine(string id) : base(id)
        {
            const string sql = "SELECT ForwardThrust, ReverseThrust, BoostThrust, BoostDuration, BoostReleaseTime, TravelThrust, TravelReleaseTime FROM Engine WHERE EquipmentID = :EquipmentID";

            (
                ForwardThrust,
                ReverseThrust,
                BoostThrust,
                BoostDuration,
                BoostReleaseTime,
                TravelThrust,
                TravelReleaseTime
            ) = X4Database.Instance.QuerySingle<(double, double, double, double, double, double, double)>(sql, new { EquipmentID = id });
        }
    }
}
