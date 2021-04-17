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
        /// 抗力(前方)
        /// </summary>
        public double DragForward { get; }


        /// <summary>
        /// 抗力(後方)
        /// </summary>
        public double DragReverse { get; }


        /// <summary>
        /// 抗力(水平)
        /// </summary>
        public double DragHorizontal { get; }


        /// <summary>
        /// 抗力(垂直)
        /// </summary>
        public double DragVertical { get; }


        /// <summary>
        /// 抗力(ピッチ)
        /// </summary>
        public double DragPitch { get; }


        /// <summary>
        /// 抗力(ヨー)
        /// </summary>
        public double DragYaw { get; }


        /// <summary>
        /// 抗力(ロール)
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
        /// サムネ画像
        /// </summary>
        public byte[]? Thumbnail { get; }
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
        /// <param name="dragForward">抗力(前方)</param>
        /// <param name="dragReverse">抗力(後方)</param>
        /// <param name="dragHorizontal">抗力(水平)</param>
        /// <param name="dragVertical">抗力(垂直)</param>
        /// <param name="dragPitch">抗力(ピッチ)</param>
        /// <param name="dragYaw">抗力(ヨー)</param>
        /// <param name="dragRoll">抗力(ロール)</param>
        /// <param name="inertiaPitch">慣性(ピッチ)</param>
        /// <param name="inertiaYaw">慣性(ヨー)</param>
        /// <param name="inertiaRoll">慣性(ロール)</param>
        /// <param name="hull">船体強度</param>
        /// <param name="people">船員数</param>
        /// <param name="missileStorage">ミサイル搭載量</param>
        /// <param name="droneStorage">ドローン搭載量</param>
        /// <param name="cargoSize">カーゴサイズ</param>
        /// <param name="minPrice">最安値</param>
        /// <param name="avgPrice">平均値</param>
        /// <param name="maxPrice">最高値</param>
        /// <param name="description">説明文</param>
        /// <param name="thumbnail">サムネ画像</param>
        public Ship(
            string shipID,
            string shipTypeID,
            string macro,
            string sizeID,
            double mass,
            double dragForward,
            double dragReverse,
            double dragHorizontal,
            double dragVertical,
            double dragPitch,
            double dragYaw,
            double dragRoll,
            double inertiaPitch,
            double inertiaYaw,
            double inertiaRoll,
            long hull,
            long people,
            long missileStorage,
            long droneStorage,
            long cargoSize,
            byte[]? thumbnail)
        {
            ShipID = shipID;
            ShipTypeID = shipTypeID;
            Macro = macro;
            SizeID = sizeID;
            Mass = mass;
            DragForward = dragForward;
            DragReverse = dragReverse;
            DragHorizontal = dragHorizontal;
            DragVertical = dragVertical;
            DragPitch = dragPitch;
            DragYaw = dragYaw;
            DragRoll = dragRoll;
            InertiaPitch = inertiaPitch;
            InertiaYaw = inertiaYaw;
            InertiaRoll = inertiaRoll;
            Hull = hull;
            People = people;
            MissileStorage = missileStorage;
            DroneStorage = droneStorage;
            CargoSize = cargoSize;
            Thumbnail = thumbnail;
        }
    }
}
