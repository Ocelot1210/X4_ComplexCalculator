using System;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// 装備種別情報用クラス
/// </summary>
public class EquipmentType : IEquipmentType
{
    #region プロパティ
    /// <inheritdoc/>
    public string EquipmentTypeID { get; }


    /// <inheritdoc/>
    public string Name { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="equipmentTypeID"></param>
    /// <param name="name"></param>
    public EquipmentType(string equipmentTypeID, string name)
    {
        EquipmentTypeID = equipmentTypeID;
        Name = name;
    }


    /// <summary>
    /// 比較
    /// </summary>
    /// <param name="obj">比較対象</param>
    /// <returns></returns>
    public override bool Equals(object? obj) => obj is IEquipmentType other && other.EquipmentTypeID == EquipmentTypeID;


    public bool Equals(IEquipmentType other) => other.EquipmentTypeID == EquipmentTypeID;


    /// <summary>
    /// ハッシュ値を取得
    /// </summary>
    /// <returns>ハッシュ値</returns>
    public override int GetHashCode() => HashCode.Combine(EquipmentTypeID);
}
