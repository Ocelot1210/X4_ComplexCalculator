namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェアの装備のタグ情報
    /// </summary>
    /// <param name="WareID">ウェアID</param>
    /// <param name="ConnectionName">コネクション名</param>
    /// <param name="Tag">タグの値</param>
    public sealed record WareEquipmentTag(string WareID, string ConnectionName, string Tag);
}
