namespace X4_DataExporterWPF.Entity;

/// <summary>
/// 派閥
/// </summary>
/// <param name="FactionID">派閥ID</param>
/// <param name="Name">派閥名</param>
/// <param name="RaceID">種族ID</param>
/// <param name="ShortName">派閥略称</param>
/// <param name="Description">説明文</param>
/// <param name="Icon">アイコン画像</param>
public sealed record Faction(
    string FactionID,
    string Name,
    string RaceID,
    string ShortName,
    string Description,
    byte[]? Icon
);
