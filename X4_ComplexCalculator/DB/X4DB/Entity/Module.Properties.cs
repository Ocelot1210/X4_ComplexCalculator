using System.Collections.Generic;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

public sealed partial class Module
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


    #region IMacro
    /// <inheritdoc/>
    public string MacroName { get; } = macro;
    #endregion


    #region IX4Module
    /// <inheritdoc/>
    public IModuleType ModuleType { get; } = moduleType;


    /// <inheritdoc/>
    public long MaxWorkers { get; } = maxWorkers;


    /// <inheritdoc/>
    public long WorkersCapacity { get; } = workersCapacity;


    /// <inheritdoc/>
    public bool NoBluePrint { get; } = noBluePrint;


    /// <inheritdoc/>
    public IReadOnlyList<IModuleProduct> Products { get; } = products;


    /// <inheritdoc/>
    public IModuleStorage Storage { get; } = storage;
    #endregion


    #region IEquippableWare
    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IWareEquipment> Equipments { get; } = equipments;
    #endregion
}
