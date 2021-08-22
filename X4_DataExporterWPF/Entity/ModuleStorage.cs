namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュールの保管容量
    /// </summary>
    /// <param name="ModuleID">モジュールID</param>
    /// <param name="Amount">保管庫容量</param>
    public sealed record ModuleStorage(string ModuleID, long Amount);
}
