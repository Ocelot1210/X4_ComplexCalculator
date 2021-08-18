namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア
    /// </summary>
    /// <param name="WareID">ウェアID</param>
    /// <param name="WareGroupID">ウェア種別ID</param>
    /// <param name="TransportTypeID">カーゴ種別ID</param>
    /// <param name="Name">ウェア名称</param>
    /// <param name="Description">説明</param>
    /// <param name="Volume">大きさ</param>
    /// <param name="MinPrice">MinPrice</param>
    /// <param name="AvgPrice">平均価格</param>
    /// <param name="MaxPrice">MaxPrice</param>
    public sealed record Ware(
        string WareID,
        string? WareGroupID,
        string? TransportTypeID,
        string Name,
        string Description,
        long Volume,
        long MinPrice,
        long AvgPrice,
        long MaxPrice
    );
}
