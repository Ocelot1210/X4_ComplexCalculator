namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船種別
    /// </summary>
    /// <param name="ShipTypeID">艦船種別ID</param>
    /// <param name="Name">名称</param>
    /// <param name="Description">説明文</param>
    public sealed record ShipType(string ShipTypeID, string Name, string Description);
}
