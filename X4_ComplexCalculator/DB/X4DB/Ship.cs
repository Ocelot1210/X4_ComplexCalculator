using System.Collections.Generic;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 艦船情報管理用クラス
    /// </summary>
    public class Ship : Ware
    {
        #region プロパティ
        /// <summary>
        /// 艦船種別
        /// </summary>
        public ShipType ShipType { get; }


        /// <summary>
        /// マクロ名
        /// </summary>
        public string Macro { get; }


        /// <summary>
        /// 艦船サイズ
        /// </summary>
        public X4Size Size { get; }


        /// <summary>
        /// 質量
        /// </summary>
        public double Mass { get; }


        #region 抗力
        /// <summary>
        /// 前方抗力
        /// </summary>
        public double DragForward { get; }


        /// <summary>
        /// 後方抗力
        /// </summary>
        public double DragReverse { get; }


        /// <summary>
        /// 水平抗力
        /// </summary>
        public double DragHorizontal { get; }


        /// <summary>
        /// 垂直抗力
        /// </summary>
        public double DragVertical { get; }


        /// <summary>
        /// ピッチ抗力
        /// </summary>
        public double DragPitch { get; }


        /// <summary>
        /// ヨー抗力
        /// </summary>
        public double DragYaw { get; }


        /// <summary>
        /// ロール抗力
        /// </summary>
        public double DragRoll { get; }
        #endregion


        #region 慣性
        /// <summary>
        /// 慣性(ピッチ)
        /// </summary>
        public double InertiaPitch { get; }


        /// <summary>
        /// 慣性(ヨー)
        /// </summary>
        public double InertiaYaw { get; }


        /// <summary>
        /// 慣性(ロール)
        /// </summary>
        public double InertiaRoll { get; }
        #endregion


        /// <summary>
        /// 船体強度
        /// </summary>
        public long Hull { get; }


        /// <summary>
        /// 船員数
        /// </summary>
        public long People { get; }


        /// <summary>
        /// ミサイル搭載量
        /// </summary>
        public long MissileStorage { get; }


        /// <summary>
        /// ドローン搭載量
        /// </summary>
        public long DroneStorage { get; }


        /// <summary>
        /// カーゴサイズ
        /// </summary>
        public long CargoSize { get; }


        /// <summary>
        /// 艦船のハンガー情報
        /// </summary>
        public IReadOnlyDictionary<string, ShipHanger> ShipHanger { get; }


        /// <summary>
        /// ロードアウトIDをキーにしたロードアウト情報のディクショナリ
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<ShipLoadout>> Loadouts { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id"></param>
        public Ship(string id) : base(id)
        {
            const string sql1 = @"
SELECT
    ShipTypeID, Macro, SizeID, Mass, DragForward, DragReverse,
    DragHorizontal, DragVertical, DragPitch, DragYaw, DragRoll, InertiaPitch, InertiaYaw, InertiaRoll,
    Hull, People, MissileStorage, DroneStorage, CargoSize
FROM
    Ship
WHERE
    ShipID = :ShipID
";

            string shipTypeID, sizeID;
            (
                shipTypeID,
                Macro,
                sizeID,
                Mass,
                DragForward,
                DragReverse,
                DragHorizontal,
                DragVertical,
                DragPitch,
                DragYaw,
                DragRoll,
                InertiaPitch,
                InertiaYaw,
                InertiaRoll,
                Hull,
                People,
                MissileStorage,
                DroneStorage,
                CargoSize
            ) = X4Database.Instance.QuerySingle
            <(
                string, string, string, long,
                double, double, double, double, double, double, double, double, double, double, 
                long, long, long, long, long
            )>(sql1, new { ShipID = id });

            ShipType = ShipType.Get(shipTypeID);
            Size = X4Size.Get(sizeID);
            ShipHanger = X4DB.ShipHanger.Get(id);


            Loadouts = ShipLoadout.Get(id)
                .GroupBy(x => x.LoadoutID)
                .ToDictionary(x => x.Key, x => x.ToArray() as IReadOnlyList<ShipLoadout>);
        }



        /// <summary>
        /// 指定したコネクション名に装備可能なEquipmentを取得する
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<T> GetEquippableEquipment<T>(string connectionName)
        {
            // 指定したコネクション名に装備可能な装備は存在するか？
            if (Equipments.TryGetValue(connectionName, out var wareEquipment))
            {
                // デフォルトのロードアウトは存在するか？
                if (Loadouts.TryGetValue("default", out var loadouts))
                {
                    // デフォルトのロードアウトの内、指定したコネクション名と同じグループ名を持つものは存在するか？
                    var shipLoadout = loadouts.FirstOrDefault(x => x.GroupName == wareEquipment.GroupName);
                    if (shipLoadout is not null)
                    {
                        // 同じグループ名の装備は指定した型と一致するか？
                        if (shipLoadout.Equipment is T ret)
                        {
                            yield return ret;
                        }
                        else
                        {
                            yield break;
                        }
                    }
                }

                var equipments = _Wares.Values
                    .OfType<T>()
                    .Where(x => !x.Tags.Except(wareEquipment.Tags).Any());
                foreach (var equipment in equipments)
                {
                    yield return equipment;
                }
            }
        }
    }
}
