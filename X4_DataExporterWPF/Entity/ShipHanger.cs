namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船のハンガー情報
    /// </summary>
    /// <param name="ShipID">艦船ID</param>
    /// <param name="SizeID">発着パッドのサイズID</param>
    /// <param name="Count">発着パッド数</param>
    /// <param name="Capacity">機体格納数</param>
    public sealed record ShipHanger(string ShipID, string SizeID, long Count, long Capacity);
}
