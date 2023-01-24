namespace X4_DataExporterWPF.Entity;

/// <summary>
/// ウェア所有派閥
/// </summary>
/// <param name="WareID">ウェアID</param>
/// <param name="FactionID">派閥ID</param>
public sealed record WareOwner(string WareID, string FactionID);
