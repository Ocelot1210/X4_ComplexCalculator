using System.Collections.Generic;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

public partial class Ware
{
    #region IWare
    /// <inheritdoc/>
    public string ID { get; }


    /// <inheritdoc/>
    public string Name { get; }


    /// <inheritdoc/>
    public IWareGroup WareGroup { get; }


    /// <inheritdoc/>
    public ITransportType TransportType { get; }


    /// <inheritdoc/>
    public string Description { get; }


    /// <inheritdoc/>
    public long Volume { get; }


    /// <inheritdoc/>
    public long MinPrice { get; }


    /// <inheritdoc/>
    public long AvgPrice { get; }


    /// <inheritdoc/>
    public long MaxPrice { get; }


    /// <inheritdoc/>
    public IReadOnlyList<IFaction> Owners { get; }


    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IWareProduction> Productions { get; }


    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IReadOnlyList<IWareResource>> Resources { get; }


    /// <inheritdoc/>
    public HashSet<string> Tags { get; }


    /// <inheritdoc/>
    public IWareEffects WareEffects { get; }
    #endregion
}
