using System;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// モジュール種別情報用クラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="moduleTypeID">モジュール種別ID</param>
/// <param name="name">モジュール種別名</param>
public sealed class ModuleType(string moduleTypeID, string name) : IModuleType
{
    #region IModuleType
    /// <inheritdoc/>
    public string ModuleTypeID { get; } = moduleTypeID;

    /// <inheritdoc/>
    public string Name { get; } = name;
    #endregion


    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is IModuleType other && ModuleTypeID == other.ModuleTypeID;


    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(ModuleTypeID);
}
