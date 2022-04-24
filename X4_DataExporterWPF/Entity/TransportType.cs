namespace X4_DataExporterWPF.Entity;

/// <summary>
/// カーゴ種別
/// </summary>
/// <param name="TransportTypeID">カーゴ種別ID</param>
/// <param name="Name">カーゴ種別名</param>
public sealed record TransportType(string TransportTypeID, string Name);
