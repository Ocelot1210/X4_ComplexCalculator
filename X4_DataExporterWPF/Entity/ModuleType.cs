namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュール種別
    /// </summary>
    /// <param name="ModuleTypeID">モジュール種別ID</param>
    /// <param name="Name">モジュール種別名</param>
    public sealed record ModuleType(string ModuleTypeID, string Name);
}
