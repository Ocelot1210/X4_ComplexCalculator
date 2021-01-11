namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// スラスター情報
    /// </summary>
    public class Thruster
    {
        #region プロパティ
        /// <summary>
        /// 装備ID
        /// </summary>
        public string EquipmentID { get; }


        /// <summary>
        /// 推進(水平・垂直)？
        /// </summary>
        public double ThrustStrafe { get; }


        /// <summary>
        /// 推進(ピッチ)
        /// </summary>
        public double ThrustPitch { get; }


        /// <summary>
        /// 推進(ヨー)
        /// </summary>
        public double ThrustYaw { get; }


        /// <summary>
        /// 推進(ロール)
        /// </summary>
        public double ThrustRoll { get; }


        /// <summary>
        /// 角度(ロール)？
        /// </summary>
        public double AngularRoll { get; }


        /// <summary>
        /// 角度(ピッチ)？
        /// </summary>
        public double AngularPitch { get; }
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <param name="thrustStrafe">推進(水平・垂直)？</param>
        /// <param name="thrustPitch">推進(ピッチ)</param>
        /// <param name="thrustYaw">推進(ヨー)</param>
        /// <param name="thrustRoll">推進(ロール)</param>
        /// <param name="angularRoll">角度(ロール)？</param>
        /// <param name="angularPitch">角度(ピッチ)？</param>
        public Thruster(
            string equipmentID,
            double thrustStrafe,
            double thrustPitch,
            double thrustYaw,
            double thrustRoll,
            double angularRoll,
            double angularPitch
            )
        {
            EquipmentID = equipmentID;
            ThrustStrafe = thrustStrafe;
            ThrustPitch = thrustPitch;
            ThrustYaw = thrustYaw;
            ThrustRoll = thrustRoll;
            AngularRoll = angularRoll;
            AngularPitch = angularPitch;
        }
    }
}
