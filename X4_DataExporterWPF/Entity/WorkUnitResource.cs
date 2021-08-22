namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 従業員が必要とするウェア情報
    /// </summary>
    /// <param name="WorkUnitID">労働種別ID</param>
    /// <param name="Method">労働方式</param>
    /// <param name="WareID">労働に必要なウェアID</param>
    /// <param name="Amount">労働に必要なウェア数量</param>
    public sealed record WorkUnitResource(
        string WorkUnitID,
        string Method,
        string WareID,
        long Amount
    );
}
