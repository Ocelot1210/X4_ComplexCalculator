using System;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// 装備種別情報用クラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="equipmentTypeID"></param>
/// <param name="name"></param>
public sealed class EquipmentType(string equipmentTypeID, string name) : IEquipmentType
{
    #region プロパティ
    /// <inheritdoc/>
    public string EquipmentTypeID { get; } = equipmentTypeID;


    /// <inheritdoc/>
    public string Name { get; } = name;
    #endregion


    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is IEquipmentType other && other.EquipmentTypeID == EquipmentTypeID;

    /// <inheritdoc/>
    public bool Equals(IEquipmentType other) => other.EquipmentTypeID == EquipmentTypeID;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(EquipmentTypeID);
}
