namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェアのタグ情報
    /// </summary>
    /// <param name="WareID">ウェアID</param>
    /// <param name="Tag">タグ</param>
    public sealed record WareTag(string WareID, string Tag);
}
