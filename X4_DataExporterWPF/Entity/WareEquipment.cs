namespace X4_DataExporterWPF.Entity;

/// <summary>
/// ウェアの装備情報
/// </summary>
/// <param name="WareID">ウェアID</param>
/// <param name="ConnectionName">コネクション名</param>
/// <param name="EquipmentTypeID">装備種別</param>
/// <param name="GroupName">グループ名</param>
public sealed record WareEquipment(
    string WareID,
    string ConnectionName,
    string EquipmentTypeID,
    string GroupName
);
