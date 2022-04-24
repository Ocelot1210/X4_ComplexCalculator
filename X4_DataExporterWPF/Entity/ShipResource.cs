namespace X4_DataExporterWPF.Entity;

/// <summary>
/// 艦船製造に必要なウェア情報
/// </summary>
/// <param name="ShipID">装備ID</param>
/// <param name="Method">装備作成方法</param>
/// <param name="WareID">必要ウェアID</param>
/// <param name="Amount">必要ウェア数</param>
public sealed record ShipResource(string ShipID, string Method, string WareID, int Amount);
