namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア生産に必要なウェア情報
    /// </summary>
    /// <param name="WareID">生産対象ウェアID</param>
    /// <param name="Method">生産方式</param>
    /// <param name="NeedWareID">生産に必要なウェアID</param>
    /// <param name="Amount">生産に必要なウェア数</param>
    public sealed record WareResource(string WareID, string Method, string NeedWareID, long Amount);
}
