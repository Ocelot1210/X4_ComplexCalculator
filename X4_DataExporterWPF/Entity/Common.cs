namespace X4_DataExporterWPF.Entity;

/// <summary>
/// データベースメタデータ
/// </summary>
/// <param name="Item">項目名</param>
/// <param name="Value">値</param>
public sealed record Common(string Item, long Value);
