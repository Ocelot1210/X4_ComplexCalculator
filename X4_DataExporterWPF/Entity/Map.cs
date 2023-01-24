namespace X4_DataExporterWPF.Entity;

/// <summary>
/// セクターマップ情報
/// </summary>
/// <param name="Macro">マクロ名</param>
/// <param name="Name">セクター名</param>
/// <param name="Description">説明</param>
public sealed record Map(string Macro, string Name, string Description);
