using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Ships
{
    /// <summary>
    /// サイズ単位のスラスターを管理するクラス
    /// </summary>
    class ThrusterManager
    {
        #region プロパティ
        /// <summary>
        /// 水平・垂直推進力が最大のスラスター
        /// </summary>
        public Thruster MaxStrafeThruster { get; }


        /// <summary>
        /// ピッチ推進力が最大のスラスター
        /// </summary>
        public Thruster MaxPitchThruster { get; }


        /// <summary>
        /// ヨー推進力が最大のスラスター
        /// </summary>
        public Thruster MaxYawThruster { get; }


        /// <summary>
        /// ロール推進力が最大のスラスター
        /// </summary>
        public Thruster MaxRollThruster { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="thrusters">スラスター一覧</param>
        public ThrusterManager(IEnumerable<Thruster> thrusters)
        {
#if DEBUG
            // サイズ違いのスラスターが混じってたら例外を投げる
            if (1 < thrusters.GroupBy(x => x.Size.SizeID).Count())
            {
                throw new ArgumentException("The size of the engine is not unified.", nameof(thrusters));
            }
#endif

            MaxStrafeThruster = thrusters.OrderBy(x => x.ThrustStrafe).First();
            MaxPitchThruster  = thrusters.OrderBy(x => x.ThrustPitch).First();
            MaxYawThruster    = thrusters.OrderBy(x => x.ThrustYaw).First();
            MaxRollThruster   = thrusters.OrderBy(x => x.ThrustRoll).First();
        }


        /// <summary>
        /// コンストラクタ(デフォルトのロードアウトがある艦船専用)
        /// </summary>
        /// <param name="thruster">スラスター</param>
        public ThrusterManager(Thruster thruster)
        {
            MaxStrafeThruster =
            MaxPitchThruster =
            MaxYawThruster =
            MaxRollThruster = thruster;
        }
    }
}
