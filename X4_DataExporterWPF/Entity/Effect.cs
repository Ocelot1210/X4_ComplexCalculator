namespace X4_DataExporterWPF.Entity;

/// <summary>
/// ウェア生産時の追加効果
/// </summary>
/// <param name="EffectID">効果ID</param>
/// <param name="Name">効果名</param>
public sealed record Effect(string EffectID, string Name);
