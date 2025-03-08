using System.Collections.Generic;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// エンジン情報用クラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="equipment">装備情報</param>
/// <param name="thrust">推進力情報</param>
/// <param name="boostDuration">ブースト持続時間</param>
/// <param name="boostReleaseTime">ブースト解除時間</param>
/// <param name="travelReleaseTime">トラベル解除時間</param>
public sealed class Engine(
    IEquipment equipment,
    EngineThrust thrust,
    double boostDuration,
    double boostReleaseTime,
    double travelReleaseTime
    ) : IEngine
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


    #region IEngine
    /// <inheritdoc/>
    public EngineThrust Thrust { get; } = thrust;


    /// <inheritdoc/>
    public double BoostDuration { get; } = boostDuration;


    /// <inheritdoc/>
    public double BoostReleaseTime { get; } = boostReleaseTime;


    /// <inheritdoc/>
    public double TravelReleaseTime { get; } = travelReleaseTime;
    #endregion
}
