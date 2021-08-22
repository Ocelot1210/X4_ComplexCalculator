namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 従業員用生産情報
    /// </summary>
    /// <param name="WorkUnitID">労働種別ID</param>
    /// <param name="Time">労働時間</param>
    /// <param name="Amount">労働時間に対して必要なウェア数量</param>
    /// <param name="Method">労働方式</param>
    public sealed record WorkUnitProduction(
        string WorkUnitID,
        long Time,
        long Amount,
        string Method
    );
}
