using System;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// ウェア種別(グループ)情報用クラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="wareGroupID">ウェアグループID</param>
/// <param name="name">ウェアグループ名</param>
/// <param name="Tier">階級</param>
public sealed class WareGroup(string wareGroupID, string name, long tier) : IWareGroup
{
    #region プロパティ
    /// <inheritdoc/>
    public string WareGroupID { get; } = wareGroupID;


    /// <inheritdoc/>
    public string Name { get; } = name;


    /// <inheritdoc/>
    public long Tier { get; } = tier;
    #endregion


    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is IWareGroup other && other.WareGroupID == WareGroupID;


    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(WareGroupID);
}
