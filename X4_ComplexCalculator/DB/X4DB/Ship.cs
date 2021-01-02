using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 艦船情報管理用クラス
    /// </summary>
    public class Ship
    {
        #region スタティックメンバ
        /// <summary>
        /// 艦船一覧
        /// </summary>
        private static readonly Dictionary<string, Ship> _Ships = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ShipID { get; }


        /// <summary>
        /// 艦船種別
        /// </summary>
        public ShipType ShipType { get; }


        /// <summary>
        /// 艦船名称
        /// </summary>
        public string Name { get; }


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
        public double ForwardDrag { get; }


        /// <summary>
        /// 後方抗力
        /// </summary>
        public double ReverseDrag { get; }


        /// <summary>
        /// 水平抗力
        /// </summary>
        public double HorizontalDrag { get; }


        /// <summary>
        /// 垂直抗力
        /// </summary>
        public double VerticalDrag { get; }


        /// <summary>
        /// ピッチ抗力
        /// </summary>
        public double PitchDrag { get; }


        /// <summary>
        /// ヨー抗力
        /// </summary>
        public double YawDrag { get; }


        /// <summary>
        /// ロール抗力
        /// </summary>
        public double RollDrag { get; }
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
        /// 最安値
        /// </summary>
        public long MinPrice { get; }


        /// <summary>
        /// 平均値
        /// </summary>
        public long AvgPrice { get; }


        /// <summary>
        /// 最高値
        /// </summary>
        public long MaxPrice { get; }


        /// <summary>
        /// 説明文
        /// </summary>
        public string Description { get; }


        /// <summary>
        /// 艦船のハンガー情報
        /// </summary>
        public IReadOnlyDictionary<string, ShipHanger> ShipHanger { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <param name="shipTypeID">艦船種別ID</param>
        /// <param name="name">艦船名称</param>
        /// <param name="macro">マクロ名</param>
        /// <param name="sizeID">サイズID</param>
        /// <param name="mass">質量</param>
        /// <param name="forwardDrag">前方抗力</param>
        /// <param name="reverseDrag">後方抗力</param>
        /// <param name="horizontalDrag">水平抗力</param>
        /// <param name="verticalDrag">垂直抗力</param>
        /// <param name="pitchDrag">ピッチ抗力</param>
        /// <param name="yawDrag">ヨー抗力</param>
        /// <param name="rollDrag">ロール抗力</param>
        /// <param name="hull">船体強度</param>
        /// <param name="people">船員数</param>
        /// <param name="missileStorage">ミサイル搭載量</param>
        /// <param name="droneStorage">ドローン搭載量</param>
        /// <param name="cargoSize">カーゴサイズ</param>
        /// <param name="minPrice">最安値</param>
        /// <param name="avgPrice">平均値</param>
        /// <param name="maxPrice">最高値</param>
        /// <param name="description">説明文</param>
        private Ship(
            string shipID,
            string shipTypeID,
            string name,
            string macro,
            string sizeID,
            double mass,
            double forwardDrag,
            double reverseDrag,
            double horizontalDrag,
            double verticalDrag,
            double pitchDrag,
            double yawDrag,
            double rollDrag,
            long hull,
            long people,
            long missileStorage,
            long droneStorage,
            long cargoSize,
            long minPrice,
            long avgPrice,
            long maxPrice,
            string description)
        {
            ShipID = shipID;
            ShipType = ShipType.Get(shipTypeID);
            Name = name;
            Macro = macro;
            Size = X4Size.Get(sizeID);
            Mass = mass;
            ForwardDrag = forwardDrag;
            ReverseDrag = reverseDrag;
            HorizontalDrag = horizontalDrag;
            VerticalDrag = verticalDrag;
            PitchDrag = pitchDrag;
            YawDrag = yawDrag;
            RollDrag = rollDrag;
            Hull = hull;
            People = people;
            MissileStorage = missileStorage;
            DroneStorage = droneStorage;
            CargoSize = cargoSize;
            MinPrice = minPrice;
            AvgPrice = avgPrice;
            MaxPrice = maxPrice;
            Description = description;
            ShipHanger = X4DB.ShipHanger.Get(shipID);
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _Ships.Clear();

            const string sql = @"SELECT ShipID, ShipTypeID, Name, Macro, SizeID, Mass, ForwardDrag, ReverseDrag,
                                        HorizontalDrag, VerticalDrag, PitchDrag, YawDrag, RollDrag, Hull, People,
                                        MissileStorage, DroneStorage, CargoSize, MinPrice, AvgPrice, MaxPrice, Description
                                FROM Ship";
            foreach (var ship in X4Database.Instance.Query<Ship>(sql))
            {
                _Ships.Add(ship.ShipID, ship);
            }
        }


        /// <summary>
        /// 艦船IDに対応する艦船を取得
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <returns>艦船IDに対応する艦船</returns>
        public static Ship Get(string shipID) => _Ships[shipID];


        /// <summary>
        /// 艦船情報を全取得する
        /// </summary>
        /// <returns>艦船情報の列挙</returns>
        public static IEnumerable<Ship> GetAll() => _Ships.Values;
    }
}
