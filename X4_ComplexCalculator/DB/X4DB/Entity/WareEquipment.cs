using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// ウェアの装備情報用クラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="wareID">ウェアID</param>
/// <param name="connectionName">コネクション名</param>
/// <param name="equipmentTypeID">装備種別ID</param>
/// <param name="groupName">グループ名</param>
/// <param name="tags">タグ一覧</param>
public sealed class WareEquipment(string wareID, string connectionName, string equipmentTypeID, string groupName, HashSet<string> tags) : IWareEquipment
{
    #region IWareEquipment
    /// <inheritdoc/>
    public string ID { get; } = wareID;


    /// <inheritdoc/>
    public string ConnectionName { get; } = connectionName;


    /// <inheritdoc/>
    public IEquipmentType EquipmentType { get; } = X4Database.Instance.EquipmentType.Get(equipmentTypeID);


    /// <inheritdoc/>
    public string GroupName { get; } = groupName;


    /// <inheritdoc/>
    public HashSet<string> Tags { get; } = tags;
    #endregion


    /// <<inheritdoc/>
    public bool CanEquipped(IEquipment equipment)
    {
        return equipment switch
        {
            IThruster => !equipment.EquipmentTags.Where(x => x != "component" && x != "thruster").Except(Tags).Any(),
            _ => !equipment.EquipmentTags.Where(x => x != "component").Except(Tags).Any(),
        };
    }


    public override int GetHashCode()
        => HashCode.Combine(ID, GroupName, ConnectionName);
}
