using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB;

/// <summary>
/// 艦船種別情報用クラス
/// </summary>
/// <param name="ShipTypeID">艦船種別ID</param>
/// <param name="Name">名称</param>
/// <param name="Description">説明文</param>
public sealed record ShipType(string ShipTypeID, string Name, string Description) : IShipType;
