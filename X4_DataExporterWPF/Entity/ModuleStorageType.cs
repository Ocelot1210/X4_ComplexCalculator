namespace X4_DataExporterWPF.Entity;

/// <summary>
/// モジュールの保管庫種別
/// </summary>
/// <param name="ModuleID">モジュールID</param>
/// <param name="TransportTypeID">保管庫種別</param>
public sealed record ModuleStorageType(string ModuleID, string TransportTypeID);
