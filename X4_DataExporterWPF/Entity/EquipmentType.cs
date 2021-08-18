namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 装備種別
    /// </summary>
    /// <param name="EquipmentTypeID">装備種別ID</param>
    /// <param name="Name">装備種別名</param>
    public sealed record EquipmentType(string EquipmentTypeID, string Name);
}
