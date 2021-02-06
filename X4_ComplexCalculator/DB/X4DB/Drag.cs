namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 抗力情報
    /// </summary>
    public class Drag
    {
        #region プロパティ
        /// <summary>
        /// 前方抗力
        /// </summary>
        public double Forward { get; }


        /// <summary>
        /// 後方抗力
        /// </summary>
        public double Reverse { get; }


        /// <summary>
        /// 水平抗力
        /// </summary>
        public double Horizontal { get; }


        /// <summary>
        /// 垂直抗力
        /// </summary>
        public double Vertical { get; }


        /// <summary>
        /// ピッチ抗力
        /// </summary>
        public double Pitch { get; }


        /// <summary>
        /// ヨー抗力
        /// </summary>
        public double Yaw { get; }


        /// <summary>
        /// ロール抗力
        /// </summary>
        public double Roll { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="forward">前方抗力</param>
        /// <param name="reverse">後方抗力</param>
        /// <param name="horizontal">水平抗力</param>
        /// <param name="vertical">垂直抗力</param>
        /// <param name="pitch">ピッチ抗力</param>
        /// <param name="yaw">ヨー抗力</param>
        /// <param name="roll">ロール抗力</param>
        public Drag(
            double forward,
            double reverse,
            double horizontal,
            double vertical,
            double pitch,
            double yaw,
            double roll
        )
        {
            Forward = forward;
            Reverse = reverse;
            Horizontal = horizontal;
            Vertical = vertical;
            Pitch = pitch;
            Yaw = yaw;
            Roll = roll;
        }
    }
}
