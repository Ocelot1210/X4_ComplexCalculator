using System;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// 種族情報用クラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="raceID">種族ID</param>
/// <param name="name">種族名</param>
/// <param name="shortName">略称</param>
/// <param name="description">説明文</param>
public sealed class Race(string raceID, string name, string shortName, string description) : IRace
{
    #region IRace
    /// <inheritdoc/>
    public string RaceID { get; } = raceID;


    /// <inheritdoc/>
    public string Name { get; } = name;


    /// <inheritdoc/>
    public string ShortName { get; } = shortName;


    /// <inheritdoc/>
    public string Description { get; } = description;
    #endregion


    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is IRace other && other.RaceID == RaceID;


    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(RaceID);
}
