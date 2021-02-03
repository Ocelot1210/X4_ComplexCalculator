namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// スラスター情報を管理するクラス
    /// </summary>
    public class Thruster : Equipment
    {
        #region プロパティ
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
        /// <param name="id">装備ID</param>
        internal Thruster(string id, string tags) : base(id, tags)
        {
            const string sql = "SELECT ThrustStrafe, ThrustPitch, ThrustYaw, ThrustRoll, AngularRoll, AngularPitch FROM Thruster WHERE EquipmentID = :EquipmentID";

            (
                ThrustStrafe,
                ThrustPitch,
                ThrustYaw,
                ThrustRoll,
                AngularRoll,
                AngularPitch
            ) = X4Database.Instance.QuerySingle<(double, double, double, double, double, double)>(sql, new { EquipmentID = id });
        }
    }
}
