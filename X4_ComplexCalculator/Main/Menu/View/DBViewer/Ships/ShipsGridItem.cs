using Prism.Mvvm;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Ships
{
    /// <summary>
    /// 艦船一覧表示用DataGridの1レコード分
    /// </summary>
    class ShipsGridItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 最高速度のエンジンと個数のペア
        /// </summary>
        private readonly IReadOnlyList<(Engine, long)> _MaxForwardEngines;


        /// <summary>
        /// 最高後退速度のエンジンと個数のペア
        /// </summary>
        private readonly IReadOnlyList<(Engine, long)> _MaxReverseEngines;


        /// <summary>
        /// 最高ブースト速度のエンジンと個数のペア
        /// </summary>
        private readonly IReadOnlyList<(Engine, long)> _MaxBoostEngines;


        /// <summary>
        /// 最高トラベル速度のエンジンと個数のペア
        /// </summary>
        private readonly IReadOnlyList<(Engine, long)> _MaxTravelEngines;


        /// <summary>
        /// 最大加速のエンジン
        /// </summary>
        private readonly IReadOnlyList<(Engine, long)> _MaxAccelerateEngines;


        /// <summary>
        /// 装備したシールド一覧
        /// </summary>
        private readonly IReadOnlyList<(Shield, long)> _Shields;
        #endregion


        #region プロパティ
        /// <summary>
        /// 艦船情報
        /// </summary>
        public Ship Ship { get; }


        /// <summary>
        /// 最高速度
        /// </summary>
        public double MaxForwardSpeed { get; }


        /// <summary>
        /// 最高後退速度
        /// </summary>
        public double MaxReverseSpeed { get; }


        /// <summary>
        /// 最高ブースト速度
        /// </summary>
        public double MaxBoostSpeed { get; }


        /// <summary>
        /// 最高トラベル速度
        /// </summary>
        public double MaxTravelSpeed { get; }


        /// <summary>
        /// 最大加速
        /// </summary>
        public double MaxAcceleration { get; }


        /// <summary>
        /// シールド容量
        /// </summary>
        public long MaxShieldCapacity { get; }


        /// <summary>
        /// シールド搭載数
        /// </summary>
        public int ShieldsCount { get; }
        #endregion




        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ship">艦船情報</param>
        /// <param name="bestEngines">最高性能のエンジン一覧</param>
        /// <param name="maxCapacityShields">サイズごとの最大容量のシールド</param>
        public ShipsGridItem(
            Ship ship,
            IReadOnlyDictionary<string, EngineManager> bestEngines,
            IReadOnlyDictionary<string, Shield> maxCapacityShields)
        {
            Ship = ship;

            var equipment = ShipEquipment.Get(ship.ShipID);

            // エンジンを設定
            {
                // 装備可能なエンジン一覧
                var engines = equipment["engines"];

                // 最高速度
                _MaxForwardEngines = engines
                    .Select(x => (bestEngines[x.Key].MaxForwardEngine, x.Value.Count))
                    .OrderBy(x => x.MaxForwardEngine.Size)
                    .ToArray();
                MaxForwardSpeed = Math.Round(_MaxForwardEngines.Sum(x => x.Item1.ForwardThrust * x.Item2) / ship.DragForward, 1);

                // 最高後退速度
                _MaxReverseEngines = engines
                    .Select(x => (bestEngines[x.Key].MaxReverseSpeedEngine, x.Value.Count))
                    .OrderBy(x => x.MaxReverseSpeedEngine.Size)
                    .ToArray();
                MaxReverseSpeed = Math.Round(_MaxReverseEngines.Sum(x => x.Item1.ReverseThrust * x.Item2) / ship.DragReverse, 1);

                // 最高ブースト速度
                _MaxBoostEngines = engines
                    .Select(x => (bestEngines[x.Key].MaxBoostSpeedEngine, x.Value.Count))
                    .OrderBy(x => x.MaxBoostSpeedEngine.Size)
                    .ToArray();
                MaxBoostSpeed = Math.Round(_MaxBoostEngines.Sum(x => x.Item1.BoostThrust * x.Item2) / ship.DragForward, 1);

                // 最高トラベル速度
                _MaxTravelEngines = engines
                    .Select(x => (bestEngines[x.Key].MaxTravelSpeedEngine, x.Value.Count))
                    .OrderBy(x => x.MaxTravelSpeedEngine.Size)
                    .ToArray();
                MaxTravelSpeed = Math.Round(_MaxTravelEngines.Sum(x => x.Item1.TravelThrust * x.Item2) / ship.DragForward, 1);


                // 最大加速
                _MaxAccelerateEngines = engines
                    .Select(x => (bestEngines[x.Key].MaxAccelerateEngine, x.Value.Count))
                    .OrderBy(x => x.MaxAccelerateEngine.Size)
                    .ToArray();
                MaxAcceleration = Math.Round(_MaxAccelerateEngines.Sum(x => x.Item1.ForwardThrust * x.Item2) / ship.Mass, 1);
            }


            // シールドを設定
            {
                _Shields = equipment["shields"]
                    .Select(x => (maxCapacityShields[x.Key], x.Value.Count))
                    .OrderBy(x => x.Item1.Size)
                    .ToArray();
                MaxShieldCapacity = _Shields.Sum(x => x.Item1.Capacity * x.Item2);

                ShieldsCount = (int)_Shields.Sum(x => x.Item2);
            }
            
        }
    }
}
