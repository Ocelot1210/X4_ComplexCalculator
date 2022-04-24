namespace X4_DataExporterWPF.Entity;

/// <summary>
/// ウェア生産時の情報
/// </summary>
/// <param name="WareID">ウェアID</param>
/// <param name="Method">ウェア生産方式</param>
/// <param name="Name">ウェア生産方式名</param>
/// <param name="Amount">生産量</param>
/// <param name="Time">生産にかかる時間</param>
public sealed record WareProduction(
    string WareID,
    string Method,
    string Name,
    long Amount,
    double Time
);
