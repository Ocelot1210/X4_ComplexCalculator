namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// 艦船の慣性情報用クラス
    /// </summary>
    public class Inertia
    {
        #region プロパティ
        /// <summary>
        /// ピッチ
        /// </summary>
        public double Pitch { get; }


        /// <summary>
        /// ヨー
        /// </summary>
        public double Yaw { get; }


        /// <summary>
        /// ロール
        /// </summary>
        public double Roll { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="pitch">ピッチ</param>
        /// <param name="yaw">ヨー</param>
        /// <param name="roll">ロール</param>
        public Inertia(double pitch, double yaw, double roll)
        {
            Pitch = pitch;
            Yaw = yaw;
            Roll = roll;
        }
    }
}
