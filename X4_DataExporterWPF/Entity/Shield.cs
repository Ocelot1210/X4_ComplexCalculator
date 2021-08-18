namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// シールド情報
    /// </summary>
    /// <param name="EquipmentID">装備ID</param>
    /// <param name="Capacity">最大シールド容量</param>
    /// <param name="RechargeRate">再充電率</param>
    /// <param name="RechargeDelay">再充電遅延</param>
    public sealed record Shield(
        string EquipmentID,
        long Capacity,
        long RechargeRate,
        double RechargeDelay
    );
}
