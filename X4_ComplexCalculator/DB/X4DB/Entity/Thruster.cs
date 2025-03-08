using System.Collections.Generic;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// スラスター情報用クラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="equipment">装備情報</param>
/// <param name="thrustStrafe">推進力</param>
/// <param name="thrustPitch">推進力(ピッチ)</param>
/// <param name="thrustYaw">推進力(ヨー)</param>
/// <param name="thrustRoll">推進力(ロール)</param>
/// <param name="angularRoll">角度(ロール)？</param>
/// <param name="angularPitch">角度(ピッチ)？</param>
public sealed class Thruster(
    IEquipment equipment,
    double thrustStrafe,
    double thrustPitch,
    double thrustYaw,
    double thrustRoll,
    double angularRoll,
    double angularPitch
    ) : IThruster
{
    #region IWare
    /// <inheritdoc/>
    public string ID { get; } = equipment.ID;


    /// <inheritdoc/>
    public string Name { get; } = equipment.Name;


    /// <inheritdoc/>
    public IWareGroup WareGroup { get; } = equipment.WareGroup;


    /// <inheritdoc/>
    public ITransportType TransportType { get; } = equipment.TransportType;


    /// <inheritdoc/>
    public string Description { get; } = equipment.Description;


    /// <inheritdoc/>
    public long Volume { get; } = equipment.Volume;


    /// <inheritdoc/>
    public long MinPrice { get; } = equipment.MinPrice;


    /// <inheritdoc/>
    public long AvgPrice { get; } = equipment.AvgPrice;


    /// <inheritdoc/>
    public long MaxPrice { get; } = equipment.MaxPrice;


    /// <inheritdoc/>
    public IReadOnlyList<IFaction> Owners { get; } = equipment.Owners;


    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IWareProduction> Productions { get; } = equipment.Productions;


    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IReadOnlyList<IWareResource>> Resources { get; } = equipment.Resources;


    /// <inheritdoc/>
    public HashSet<string> Tags { get; } = equipment.Tags;


    /// <inheritdoc/>
    public IWareEffects WareEffects { get; } = equipment.WareEffects;
    #endregion


    #region IEquipment
    /// <inheritdoc/>
    public IEquipmentType EquipmentType { get; } = equipment.EquipmentType;


    /// <inheritdoc/>
    public long Hull { get; } = equipment.Hull;


    /// <inheritdoc/>
    public bool HullIntegrated { get; } = equipment.HullIntegrated;


    /// <inheritdoc/>
    public long Mk { get; } = equipment.Mk;


    /// <inheritdoc/>
    public IRace? MakerRace { get; } = equipment.MakerRace;


    /// <inheritdoc/>
    public HashSet<string> EquipmentTags { get; } = equipment.EquipmentTags;


    /// <inheritdoc/>
    public IX4Size? Size { get; } = equipment.Size;
    #endregion


    #region IMacro
    /// <inheritdoc/>
    public string MacroName { get; } = equipment.MacroName;
    #endregion


    #region IThruster
    /// <inheritdoc/>
    public double ThrustStrafe { get; } = thrustStrafe;


    /// <inheritdoc/>
    public double ThrustPitch { get; } = thrustPitch;


    /// <inheritdoc/>
    public double ThrustYaw { get; } = thrustYaw;


    /// <inheritdoc/>
    public double ThrustRoll { get; } = thrustRoll;


    /// <inheritdoc/>
    public double AngularRoll { get; } = angularRoll;


    /// <inheritdoc/>
    public double AngularPitch { get; } = angularPitch;
    #endregion
}
