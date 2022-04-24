namespace X4_DataExporterWPF.Entity;

/// <summary>
/// 艦船用途
/// </summary>
/// <param name="ShipID">艦船ID</param>
/// <param name="Type">用途区分(primary等)</param>
/// <param name="PurposeID">用途ID</param>
public sealed record ShipPurpose(string ShipID, string Type, string PurposeID);
