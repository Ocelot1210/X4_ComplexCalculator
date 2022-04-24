namespace X4_DataExporterWPF.Entity;

/// <summary>
/// 装備
/// </summary>
/// <param name="EquipmentID">装備ID</param>
/// <param name="MacroName">マクロ名</param>
/// <param name="EquipmentTypeID">装備種別ID</param>
/// <param name="Hull">船体値</param>
/// <param name="HullIntegrated">船体値が統合されているか</param>
/// <param name="Mk">Mk</param>
/// <param name="MakerRace">作成種族</param>
/// <param name="Thumbnail">サムネ画像</param>
public sealed record Equipment(
    string EquipmentID,
    string MacroName,
    string EquipmentTypeID,
    long Hull,
    bool HullIntegrated,
    long Mk,
    string? MakerRace,
    byte[]? Thumbnail
);
