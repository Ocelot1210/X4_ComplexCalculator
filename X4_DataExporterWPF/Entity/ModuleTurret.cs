namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// モジュールのタレット
    /// </summary>
    /// <param name="ModuleID">モジュールID</param>
    /// <param name="SizeID">サイズID</param>
    /// <param name="Amount">装備可能個数</param>
    public sealed record ModuleTurret(string ModuleID, string SizeID, long Amount);
}
