using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

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
        private readonly IReadOnlyList<(IEngine, int)> _MaxForwardEngines;


        /// <summary>
        /// 最高後退速度のエンジンと個数のペア
        /// </summary>
        private readonly IReadOnlyList<(IEngine, int)> _MaxReverseEngines;


        /// <summary>
        /// 最高ブースト速度のエンジンと個数のペア
        /// </summary>
        private readonly IReadOnlyList<(IEngine, int)> _MaxBoostEngines;


        /// <summary>
        /// 最高トラベル速度のエンジンと個数のペア
        /// </summary>
        private readonly IReadOnlyList<(IEngine, int)> _MaxTravelEngines;


        /// <summary>
        /// 最大加速のエンジン
        /// </summary>
        private readonly IReadOnlyList<(IEngine, int)> _MaxAccelerateEngines;


        /// <summary>
        /// 装備したシールド一覧
        /// </summary>
        private readonly IReadOnlyList<(IShield, int)> _EquippedShields;


        /// <summary>
        /// 平行移動が最大のスラスターと個数のペア
        /// </summary>
        private readonly IReadOnlyList<(IThruster, int)> _MaxTranslationThrusters;


        /// <summary>
        /// ピッチ推進力が最大のスラスター
        /// </summary>
        private readonly IReadOnlyList<(IThruster, int)> _MaxPitchThruster;


        /// <summary>
        /// ヨー推進力が最大のスラスター
        /// </summary>
        private readonly IReadOnlyList<(IThruster, int)> _MaxYawThruster;


        /// <summary>
        /// ロール推進力が最大のスラスター
        /// </summary>
        private readonly IReadOnlyList<(IThruster, int)> _MaxRollThruster;
        #endregion


        #region プロパティ
        /// <summary>
        /// 艦船情報
        /// </summary>
        public IShip Ship { get; }


        #region 基礎情報
        /// <summary>
        /// 艦船名称
        /// </summary>
        public string ShipName => Ship.Name;


        /// <summary>
        /// 艦船種別名称
        /// </summary>
        public string ShipTypeName => Ship.ShipType.Name;


        /// <summary>
        /// 艦船サイズ
        /// </summary>
        public IX4Size ShipSize => Ship.Size;


        /// <summary>
        /// 艦船重量
        /// </summary>
        public double ShipMass { get; }


        /// <summary>
        /// 船員数
        /// </summary>
        public long People => Ship.People;
        #endregion


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


        /// <summary>
        /// 最大加速
        /// </summary>
        public double MaxAcceleration { get; }
        #endregion


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


        /// <summary>
        /// ミサイル搭載量
        /// </summary>
        public long MissileStorage => Ship.MissileStorage;
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


        #region 保管庫
        /// <summary>
        /// 保管庫容量
        /// </summary>
        public long CargoSize => Ship.CargoSize;


        /// <summary>
        /// 保管庫種別
        /// </summary>
        public IReadOnlyCollection<string> CargoTypes { get; }
        #endregion


        #region ドック
        /// <summary>
        /// 中型ドック数
        /// </summary>
        public int MediumDockCount { get; }
        


        /// <summary>
        /// 小型ドック数
        /// </summary>
        public int SmallDockCount { get; }
        #endregion


        #region 搭載数
        /// <summary>
        /// 機体搭載量
        /// </summary>
        public int HangerCapacity { get; }


        /// <summary>
        /// ドローン搭載数
        /// </summary>
        public long DroneStorage => Ship.DroneStorage;
        #endregion
        #endregion


        public static ShipsGridItem? Create(IShip ship)
        {
            try
            {
                return new ShipsGridItem(ship);
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ship">艦船情報</param>
        public ShipsGridItem(IShip ship)
        {
            Ship = ship;
            
            // xaml上のStringFormatで丸めるとフィルターの挙動がおかしくなるため
            // あらかじめ丸めた艦船重量を作成しておく
            ShipMass = Math.Round(ship.Mass, 1);

            // エンジンを設定
            {
                var maxForwardEngines = new List<IEngine>();
                var maxReverseEngines = new List<IEngine>();
                var maxBoostEngines   = new List<IEngine>();
                var maxTravelEngines  = new List<IEngine>();

                var conNames = ship.Equipments
                    .Where(x => x.Value.EquipmentType.EquipmentTypeID == "engines")
                    .Select(x => x.Key);
                foreach (var conName in conNames)
                {
                    // 現在のコネクションに装備可能なエンジン一覧
                    var engines = ship.GetEquippableEquipment<IEngine>(conName).ToArray();
                    if (!engines.Any())
                    {
                        continue;
                    }

                    maxForwardEngines.Add(engines.OrderByDescending(x => x.Thrust.Forward).First());
                    maxReverseEngines.Add(engines.OrderByDescending(x => x.Thrust.Reverse).First());
                    maxBoostEngines.Add(engines.OrderByDescending(x => x.Thrust.Boost).First());
                    maxTravelEngines.Add(engines.OrderByDescending(x => x.Thrust.Travel).First());
                }

                // 最高速度
                _MaxForwardEngines = maxForwardEngines
                    .GroupBy(x => x)
                    .Select(x => (x.Key, x.Count()))
                    .OrderBy(x => x.Key.Name)
                    .ToArray();
                MaxForwardSpeed = Math.Round(_MaxForwardEngines.Sum(x => x.Item1.Thrust.Forward * x.Item2) / ship.Drag.Forward, 1);

                // 最高後退速度
                _MaxReverseEngines = maxReverseEngines
                    .GroupBy(x => x)
                    .Select(x => (x.Key, x.Count()))
                    .OrderBy(x => x.Key.Name)
                    .ToArray();
                MaxReverseSpeed = Math.Round(_MaxReverseEngines.Sum(x => x.Item1.Thrust.Reverse * x.Item2) / ship.Drag.Reverse, 1);

                // 最高ブースト速度
                _MaxBoostEngines = maxBoostEngines
                    .GroupBy(x => x)
                    .Select(x => (x.Key, x.Count()))
                    .OrderBy(x => x.Key.Name)
                    .ToArray();
                MaxBoostSpeed = Math.Round(_MaxBoostEngines.Sum(x => x.Item1.Thrust.Boost * x.Item2) / ship.Drag.Forward, 1);

                // 最高トラベル速度
                _MaxTravelEngines = maxTravelEngines
                    .GroupBy(x => x)
                    .Select(x => (x.Key, x.Count()))
                    .OrderBy(x => x.Key.Name)
                    .ToArray();
                MaxTravelSpeed = Math.Round(_MaxTravelEngines.Sum(x => x.Item1.Thrust.Travel * x.Item2) / ship.Drag.Forward, 1);


                // 最大加速
                _MaxAccelerateEngines = _MaxForwardEngines;
                MaxAcceleration = Math.Round(_MaxAccelerateEngines.Sum(x => x.Item1.Thrust.Forward * x.Item2) / ship.Mass, 1);
            }


            // シールドを設定
            {
                _EquippedShields = ship.Equipments
                    .Where(x => x.Value.EquipmentType.EquipmentTypeID == "shields")
                    .Select(x => ship.GetEquippableEquipment<IShield>(x.Key)
                        .OrderByDescending(y => y.Capacity)
                        .FirstOrDefault()
                    )
                    .Where(x => x is not null)
                    .Select(x => x!)
                    .GroupBy(x => x)
                    .Select(x => (x.Key, x.Count()))
                    .OrderBy(x => x.Key.Name)
                    .ToArray();

                MaxShieldCapacity = _EquippedShields.Sum(x => x.Item1.Capacity * x.Item2);
                ShieldsCount = _EquippedShields.Sum(x => x.Item2);
            }


            // スラスターを設定
            {
                var maxTranslationThrusters = new List<IThruster>();
                var maxPitchThruster        = new List<IThruster>();
                var maxYawThruster          = new List<IThruster>();
                var maxRollThruster         = new List<IThruster>();

                var conNames = ship.Equipments
                    .Where(x => x.Value.EquipmentType.EquipmentTypeID == "thrusters")
                    .Select(x => x.Key);
                foreach (var conName in conNames)
                {
                    // 現在のコネクションに装備可能なスラスター一覧
                    var thrusters = ship.GetEquippableEquipment<IThruster>(conName).ToArray();
                    if (!thrusters.Any())
                    {
                        continue;
                    }

                    maxTranslationThrusters.Add(thrusters.OrderByDescending(x => x.ThrustStrafe).First());
                    maxPitchThruster.Add(thrusters.OrderByDescending(x => x.ThrustPitch).First());
                    maxYawThruster.Add(thrusters.OrderByDescending(x => x.ThrustYaw).First());
                    maxRollThruster.Add(thrusters.OrderByDescending(x => x.ThrustRoll).First());
                }


                // 平行移動が最大のスラスター
                _MaxTranslationThrusters = maxTranslationThrusters
                    .GroupBy(x => x)
                    .Select(x => (x.Key, x.Count()))
                    .OrderBy(x => x.Key.Name)
                    .ToArray();
                {
                    var totalThrust = _MaxTranslationThrusters.Sum(x => x.Item1.ThrustStrafe * x.Item2);
                    VerticalMovementSpeed = Math.Round(totalThrust / ship.Drag.Vertical, 1);
                    HorizontalMovementSpeed = Math.Round(totalThrust / ship.Drag.Horizontal, 1);
                }


                // ピッチ
                _MaxPitchThruster = maxPitchThruster
                    .GroupBy(x => x)
                    .Select(x => (x.Key, x.Count()))
                    .OrderBy(x => x.Key.Name)
                    .ToArray();
                PitchRate = Math.Round(_MaxPitchThruster.Sum(x => x.Item1.ThrustPitch * x.Item2) / ship.Drag.Pitch, 1);


                // ヨー
                _MaxYawThruster = maxYawThruster
                    .GroupBy(x => x)
                    .Select(x => (x.Key, x.Count()))
                    .OrderBy(x => x.Key.Name)
                    .ToArray();
                YawRate = Math.Round(_MaxYawThruster.Sum(x => x.Item1.ThrustYaw * x.Item2) / ship.Drag.Yaw, 1);


                // ロール
                _MaxRollThruster = maxRollThruster
                    .GroupBy(x => x)
                    .Select(x => (x.Key, x.Count()))
                    .OrderBy(x => x.Key.Name)
                    .ToArray();
                RollRate = Math.Round(_MaxRollThruster.Sum(x => x.Item1.ThrustRoll * x.Item2) / ship.Drag.Roll, 1);


                // 反応性
                Responsiveness = Math.Round(ship.Drag.Yaw / ship.Inertia.Yaw, 3);
            }


            // 武装
            {
                Weapons = ship.Equipments.Values.Count(x => x.EquipmentType.EquipmentTypeID == "weapons");
                Turrets = ship.Equipments.Values.Count(x => x.EquipmentType.EquipmentTypeID == "turrets");
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

                CargoTypes = X4Database.Instance.Query<string>(sql, new { ShipID = ship.ID }).ToArray();
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
