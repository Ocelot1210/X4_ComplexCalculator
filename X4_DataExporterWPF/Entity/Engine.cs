namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// エンジン情報
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// 装備ID
        /// </summary>
        public string EquipmentID { get; }


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


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <param name="forwardThrust">前方推進力</param>
        /// <param name="reverseThrust">後方推進力</param>
        /// <param name="boostThrust">ブースト推進力</param>
        /// <param name="boostDuration">ブースト持続時間</param>
        /// <param name="boostReleaseTime">ブースト解除時間</param>
        /// <param name="travelThrust">トラベル推進力</param>
        /// <param name="travelReleaseTime">トラベル解除時間</param>
        public Engine(
            string equipmentID,
            double forwardThrust,
            double reverseThrust,
            double boostThrust,
            double boostDuration,
            double boostReleaseTime,
            double travelThrust,
            double travelReleaseTime
        )
        {
            EquipmentID = equipmentID;
            ForwardThrust = forwardThrust;
            ReverseThrust = reverseThrust;
            BoostThrust = boostThrust;
            BoostDuration = boostDuration;
            BoostReleaseTime = boostReleaseTime;
            TravelThrust = travelThrust;
            TravelReleaseTime = travelReleaseTime;
        }
    }
}
