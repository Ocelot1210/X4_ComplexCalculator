namespace X4_DataExporterWPF.Entity;

/// <summary>
/// 艦船のカーゴタイプ
/// </summary>
/// <param name="ShipID">艦船ID</param>
/// <param name="TransportTypeID">カーゴ種別</param>
public sealed record ShipTransportType(string ShipID, string TransportTypeID);
