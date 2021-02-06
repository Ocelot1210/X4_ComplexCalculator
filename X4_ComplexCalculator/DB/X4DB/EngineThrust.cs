using System;
using System.Collections.Generic;
using System.Text;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// エンジンの推進力を表すクラス
    /// </summary>
    public class EngineThrust
    {
        #region プロパティ
        /// <summary>
        /// 前方推進力
        /// </summary>
        public double Forward { get; }


        /// <summary>
        /// 後方推進力
        /// </summary>
        public double Reverse { get; }


        /// <summary>
        /// ブースト推進力
        /// </summary>
        public double Boost { get; }


        /// <summary>
        /// トラベル推進力
        /// </summary>
        public double Travel { get; }
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="forward">前方推進力</param>
        /// <param name="reverse">後方推進力</param>
        /// <param name="boost">ブースト推進力</param>
        /// <param name="travel">トラベル推進力</param>
        public EngineThrust(
            double forward,
            double reverse,
            double boost,
            double travel
        )
        {
            Forward = forward;
            Reverse = reverse;
            Boost = boost;
            Travel = travel;
        }
    }
}
