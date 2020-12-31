namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船
    /// </summary>
    public class Ship
    {
        #region プロパティ
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ShipID { get; }


        /// <summary>
        /// 艦船種別ID
        /// </summary>
        public string ShipTypeID { get; }


        /// <summary>
        /// 艦船名称
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// マクロ名
        /// </summary>
        public string Macro { get; }


        /// <summary>
        /// サイズID
        /// </summary>
        public string SizeID { get; }


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
        public int Hull { get; }


        /// <summary>
        /// 船員数
        /// </summary>
        public int People { get; }


        /// <summary>
        /// ミサイル搭載量
        /// </summary>
        public int MissileStorage { get; }


        /// <summary>
        /// ドローン搭載量
        /// </summary>
        public int DroneStorage { get; }


        /// <summary>
        /// カーゴサイズ
        /// </summary>
        public int CargoSize { get; }


        /// <summary>
        /// 最安値
        /// </summary>
        public int MinPrice { get; }


        /// <summary>
        /// 平均値
        /// </summary>
        public int AvgPrice { get; }


        /// <summary>
        /// 最高値
        /// </summary>
        public int MaxPrice { get; }


        /// <summary>
        /// 説明文
        /// </summary>
        public string Description { get; }
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
        /// <param name="cagoSize">カーゴサイズ</param>
        /// <param name="minPrice">最安値</param>
        /// <param name="avgPrice">平均値</param>
        /// <param name="maxPrice">最高値</param>
        /// <param name="description">説明文</param>
        public Ship(
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
            int hull,
            int people,
            int missileStorage,
            int droneStorage,
            int cagoSize,
            int minPrice,
            int avgPrice,
            int maxPrice,
            string description)
        {
            ShipID = shipID;
            ShipTypeID = shipTypeID;
            Name = name;
            Macro = macro;
            SizeID = sizeID;
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
            CargoSize = cagoSize;
            MinPrice = minPrice;
            AvgPrice = avgPrice;
            MaxPrice = maxPrice;
            Description = description;
        }
    }
}
