namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 装備のタグ情報
    /// </summary>
    /// <param name="EquipmentID">装備ID</param>
    /// <param name="Tag">タグ</param>
    public sealed record EquipmentTag(string EquipmentID, string Tag);
}
