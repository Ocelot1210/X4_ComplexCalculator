namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュールの生産品
    /// </summary>
    /// <param name="ModuleID">モジュールID</param>
    /// <param name="WareID">生産ウェアID</param>
    /// <param name="Method">生産方式</param>
    /// <param name="Amount">生産量</param>
    public sealed record ModuleProduct(string ModuleID, string WareID, string Method, long Amount);
}
