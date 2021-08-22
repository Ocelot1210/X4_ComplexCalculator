namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船
    /// </summary>
    /// <param name="ShipID">艦船ID</param>
    /// <param name="ShipTypeID">艦船種別ID</param>
    /// <param name="Macro">マクロ名</param>
    /// <param name="SizeID">サイズID</param>
    /// <param name="Mass">質量</param>
    /// <param name="DragForward">抗力(前方)</param>
    /// <param name="DragReverse">抗力(後方)</param>
    /// <param name="DragHorizontal">抗力(水平)</param>
    /// <param name="DragVertical">抗力(垂直)</param>
    /// <param name="DragPitch">抗力(ピッチ)</param>
    /// <param name="DragYaw">抗力(ヨー)</param>
    /// <param name="DragRoll">抗力(ロール)</param>
    /// <param name="InertiaPitch">慣性(ピッチ)</param>
    /// <param name="InertiaYaw">慣性(ヨー)</param>
    /// <param name="InertiaRoll">慣性(ロール)</param>
    /// <param name="Hull">船体強度</param>
    /// <param name="People">船員数</param>
    /// <param name="MissileStorage">ミサイル搭載量</param>
    /// <param name="DroneStorage">ドローン搭載量</param>
    /// <param name="CargoSize">カーゴサイズ</param>
    /// <param name="Thumbnail">サムネ画像</param>
    public sealed record Ship(
        string ShipID,
        string ShipTypeID,
        string Macro,
        string SizeID,
        double Mass,
        double DragForward,
        double DragReverse,
        double DragHorizontal,
        double DragVertical,
        double DragPitch,
        double DragYaw,
        double DragRoll,
        double InertiaPitch,
        double InertiaYaw,
        double InertiaRoll,
        long Hull,
        long People,
        long MissileStorage,
        long DroneStorage,
        long CargoSize,
        byte[]? Thumbnail
    );
}
