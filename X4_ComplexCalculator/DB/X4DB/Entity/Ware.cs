using System;
using System.Collections.Generic;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// ウェア情報用クラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="id">ウェアID</param>
/// <param name="name">ウェア名</param>
/// <param name="wareGroup">ウェア種別</param>
/// <param name="transportType">カーゴ種別</param>
/// <param name="description">説明文</param>
/// <param name="volume">コンテナサイズ</param>
/// <param name="minPrice">最低価格</param>
/// <param name="avgPrice">平均価格</param>
/// <param name="maxPrice">最高価格</param>
/// <param name="owners">所有派閥</param>
/// <param name="productions">生産方式</param>
/// <param name="resources">生産に必要なウェア情報</param>
/// <param name="tags">タグ一覧</param>
/// <param name="wareEffects">ウェア生産時の追加効果情報</param>
public sealed class Ware(
    string id,
    string name,
    IWareGroup wareGroup,
    ITransportType transportType,
    string description,
    long volume,
    long minPrice,
    long avgPrice,
    long maxPrice,
    IReadOnlyList<IFaction> owners,
    IReadOnlyDictionary<string, IWareProduction> productions,
    IReadOnlyDictionary<string, IReadOnlyList<IWareResource>> resources,
    HashSet<string> tags,
    IWareEffects wareEffects
    ) : IWare
{
    #region IWare
    /// <inheritdoc/>
    public string ID { get; } = id;


    /// <inheritdoc/>
    public string Name { get; } = name;


    /// <inheritdoc/>
    public IWareGroup WareGroup { get; } = wareGroup;


    /// <inheritdoc/>
    public ITransportType TransportType { get; } = transportType;


    /// <inheritdoc/>
    public string Description { get; } = description;


    /// <inheritdoc/>
    public long Volume { get; } = volume;


    /// <inheritdoc/>
    public long MinPrice { get; } = minPrice;


    /// <inheritdoc/>
    public long AvgPrice { get; } = avgPrice;


    /// <inheritdoc/>
    public long MaxPrice { get; } = maxPrice;


    /// <inheritdoc/>
    public IReadOnlyList<IFaction> Owners { get; } = owners;


    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IWareProduction> Productions { get; } = productions;


    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IReadOnlyList<IWareResource>> Resources { get; } = resources;


    /// <inheritdoc/>
    public HashSet<string> Tags { get; } = tags;


    /// <inheritdoc/>
    public IWareEffects WareEffects { get; } = wareEffects;
    #endregion


    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is IWare tgt && tgt.ID == ID;


    /// <inheritdoc/>
    public bool Equals(IWare other) => ID == other.ID;


    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(ID);
}
