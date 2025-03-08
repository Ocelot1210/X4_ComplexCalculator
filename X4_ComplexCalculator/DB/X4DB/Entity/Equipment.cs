using System;
using System.Collections.Generic;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;


/// <summary>
/// 装備品情報用クラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="ware">ウェア情報</param>
/// <param name="macro">マクロ名</param>
/// <param name="equipmentType">装備種別</param>
/// <param name="hull">船体値</param>
/// <param name="hullIntegrated">船体値が統合されているか</param>
/// <param name="mk">Mk</param>
/// <param name="makerRace">製造種族</param>
/// <param name="equipmentTags">タグ情報</param>
/// <param name="size">サイズ</param>
public sealed class Equipment(
    IWare ware,
    string macro,
    IEquipmentType equipmentType,
    long hull,
    bool hullIntegrated,
    long mk,
    IRace? makerRace,
    HashSet<string> equipmentTags,
    IX4Size? size
    ) : IEquipment
{
    #region IWare
    /// <inheritdoc/>
    public string ID { get; } = ware.ID;


    /// <inheritdoc/>
    public string Name { get; } = ware.Name;


    /// <inheritdoc/>
    public IWareGroup WareGroup { get; } = ware.WareGroup;


    /// <inheritdoc/>
    public ITransportType TransportType { get; } = ware.TransportType;


    /// <inheritdoc/>
    public string Description { get; } = ware.Description;


    /// <inheritdoc/>
    public long Volume { get; } = ware.Volume;


    /// <inheritdoc/>
    public long MinPrice { get; } = ware.MinPrice;


    /// <inheritdoc/>
    public long AvgPrice { get; } = ware.AvgPrice;


    /// <inheritdoc/>
    public long MaxPrice { get; } = ware.MaxPrice;


    /// <inheritdoc/>
    public IReadOnlyList<IFaction> Owners { get; } = ware.Owners;


    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IWareProduction> Productions { get; } = ware.Productions;


    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IReadOnlyList<IWareResource>> Resources { get; } = ware.Resources;


    /// <inheritdoc/>
    public HashSet<string> Tags { get; } = ware.Tags;


    /// <inheritdoc/>
    public IWareEffects WareEffects { get; } = ware.WareEffects;
    #endregion


    #region IEquipment
    /// <inheritdoc/>
    public IEquipmentType EquipmentType { get; } = equipmentType;


    /// <inheritdoc/>
    public long Hull { get; } = hull;


    /// <inheritdoc/>
    public bool HullIntegrated { get; } = hullIntegrated;


    /// <inheritdoc/>
    public long Mk { get; } = mk;


    /// <inheritdoc/>
    public IRace? MakerRace { get; } = makerRace;


    /// <inheritdoc/>
    public HashSet<string> EquipmentTags { get; } = equipmentTags;


    /// <inheritdoc/>
    public IX4Size? Size { get; } = size;
    #endregion


    #region IMacro
    /// <inheritdoc/>
    public string MacroName { get; } = macro;
    #endregion


    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is IWare other && ID == other.ID;


    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(ID);
}
