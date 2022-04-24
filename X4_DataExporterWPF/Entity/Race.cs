namespace X4_DataExporterWPF.Entity;

/// <summary>
/// 種族
/// </summary>
/// <param name="raceID">種族ID</param>
/// <param name="name">種族名</param>
/// <param name="shortName">種族略称</param>
/// <param name="description">説明文</param>
/// <param name="icon">アイコン画像</param>
public sealed record Race(
    string RaceID,
    string Name,
    string ShortName,
    string Description,
    byte[]? Icon
);
