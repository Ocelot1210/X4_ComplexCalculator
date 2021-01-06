using Prism.Mvvm;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System;
using X4_ComplexCalculator.DB;

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
        private readonly IReadOnlyList<(Shield, long)> _EquippedShields;


        /// <summary>
        /// 平行移動が最大のスラスターと個数のペア
        /// </summary>
        private readonly IReadOnlyList<(Thruster, long)> _MaxTranslationThrusters;


        /// <summary>
        /// ピッチ推進力が最大のスラスター
        /// </summary>
        private readonly IReadOnlyList<(Thruster, long)> _MaxPitchThruster;


        /// <summary>
        /// ヨー推進力が最大のスラスター
        /// </summary>
        private readonly IReadOnlyList<(Thruster, long)> _MaxYawThruster;


        /// <summary>
        /// ロール推進力が最大のスラスター
        /// </summary>
        private readonly IReadOnlyList<(Thruster, long)> _MaxRollThruster;
        #endregion


        #region プロパティ
        /// <summary>
        /// 艦船情報
        /// </summary>
        public Ship Ship { get; }


        #region 速度
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
        #endregion


        /// <summary>
        /// 最大加速
        /// </summary>
        public double MaxAcceleration { get; }



        #region 平行移動速度
        /// <summary>
        /// 垂直移動速度
        /// </summary>
        public double VerticalMovementSpeed { get; }


        /// <summary>
        /// 水平移動速度
        /// </summary>
        public double HorizontalMovementSpeed { get; }
        #endregion


        #region 操舵性能
        /// <summary>
        /// ピッチ
        /// </summary>
        public double PitchRate { get; }


        /// <summary>
        /// ヨー
        /// </summary>
        public double YawRate { get; }


        /// <summary>
        /// ロール
        /// </summary>
        public double RollRate { get; }


        /// <summary>
        /// 反応性
        /// </summary>
        public double Responsiveness { get; }
        #endregion

        #region 武装
        /// <summary>
        /// 武器
        /// </summary>
        public int Weapons { get; }


        /// <summary>
        /// タレット
        /// </summary>
        public int Turrets { get; }
        #endregion




        #region シールド
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
        /// 保管庫種別
        /// </summary>
        public string CargoType { get; }


        /// <summary>
        /// 中型ドック数
        /// </summary>
        public int MediumDockCount { get; }


        /// <summary>
        /// 小型ドック数
        /// </summary>
        public int SmallDockCount { get; }


        /// <summary>
        /// 機体搭載量
        /// </summary>
        public int HangerCapacity { get; }
        #endregion




        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ship">艦船情報</param>
        /// <param name="bestEngines">最高性能のエンジン一覧</param>
        /// <param name="maxCapacityShields">サイズごとの最大容量のシールド</param>
        /// <param name="bestThrusters">最高性能のスラスター一覧</param>
        public ShipsGridItem(
            Ship ship,
            IReadOnlyDictionary<string, EngineManager> bestEngines,
            IReadOnlyDictionary<string, Shield> maxCapacityShields,
            IReadOnlyDictionary<string, ThrusterManager> bestThrusters)
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
                _EquippedShields = equipment["shields"]
                    .Select(x => (maxCapacityShields[x.Key], x.Value.Count))
                    .OrderBy(x => x.Item1.Size)
                    .ToArray();
                MaxShieldCapacity = _EquippedShields.Sum(x => x.Item1.Capacity * x.Item2);

                ShieldsCount = (int)_EquippedShields.Sum(x => x.Item2);
            }


            // スラスターを設定
            {
                // 装備可能なスラスター一覧
                var thrusters = equipment["thrusters"];


                // 平行移動が最大のスラスター
                _MaxTranslationThrusters = thrusters
                    .Select(x => (bestThrusters[x.Key].MaxStrafeThruster, x.Value.Count))
                    .OrderBy(x => x.MaxStrafeThruster.Size)
                    .ToArray();
                {
                    var totalThrust = _MaxTranslationThrusters.Sum(x => x.Item1.ThrustStrafe * x.Item2);
                    VerticalMovementSpeed = Math.Round(totalThrust / ship.DragVertical, 1);
                    HorizontalMovementSpeed = Math.Round(totalThrust / ship.DragHorizontal, 1);
                }


                // ピッチ
                _MaxPitchThruster = thrusters
                    .Select(x => (bestThrusters[x.Key].MaxPitchThruster, x.Value.Count))
                    .OrderBy(x => x.MaxPitchThruster.Size)
                    .ToArray();
                PitchRate = Math.Round(_MaxPitchThruster.Sum(x => x.Item1.ThrustPitch * x.Item2) / ship.DragPitch, 1);


                // ヨー
                _MaxYawThruster = thrusters
                    .Select(x => (bestThrusters[x.Key].MaxYawThruster, x.Value.Count))
                    .OrderBy(x => x.MaxYawThruster.Size)
                    .ToArray();
                YawRate = Math.Round(_MaxYawThruster.Sum(x => x.Item1.ThrustYaw * x.Item2) / ship.DragYaw, 1);


                // ロール
                _MaxRollThruster = thrusters
                    .Select(x => (bestThrusters[x.Key].MaxRollThruster, x.Value.Count))
                    .OrderBy(x => x.MaxRollThruster.Size)
                    .ToArray();
                RollRate = Math.Round(_MaxRollThruster.Sum(x => x.Item1.ThrustRoll * x.Item2) / ship.DragRoll, 1);


                // 反応性
                Responsiveness = Math.Round(ship.DragYaw / ship.InertiaYaw, 3);
            }


            // 武装
            {
                {
                    Weapons = equipment.TryGetValue("weapons", out var x) ? x.Sum(x => (int)x.Value.Count) : 0;
                }
                {
                    Turrets = equipment.TryGetValue("turrets", out var x) ? x.Sum(x => (int)x.Value.Count) : 0;
                }
            }


            // 保管庫種別
            {
                const string sql = @"
SELECT
    TransportType.Name
FROM
    TransportType, ShipTransportType
WHERE
    ShipTransportType.TransportTypeID = TransportType.TransportTypeID AND
    ShipID = :ShipID";

                CargoType = string.Join('/', X4Database.Instance.Query<string>(sql, new { ship.ShipID }));
            }


            // ドック・機体
            {
                MediumDockCount = (int)ship.ShipHanger.Where(x => x.Key == "medium").Sum(x => x.Value.Count);
                SmallDockCount  = (int)ship.ShipHanger.Where(x => x.Key == "small").Sum(x => x.Value.Count);
                HangerCapacity  = (int)ship.ShipHanger.Where(x => x.Key != "extrasmall").Sum(x => x.Value.Capacity);
            }
        }
    }
}
