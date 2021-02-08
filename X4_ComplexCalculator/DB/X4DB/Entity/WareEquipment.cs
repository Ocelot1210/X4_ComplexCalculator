using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// ウェアの装備情報
    /// </summary>
    public class WareEquipment : IWareEquipment
    {
        #region IWareEquipment
        /// <inheritdoc/>
        public string ID { get; }


        /// <inheritdoc/>
        public string ConnectionName { get; }


        /// <inheritdoc/>
        public IEquipmentType EquipmentType { get; }


        /// <inheritdoc/>
        public string GroupName { get; }


        /// <inheritdoc/>
        public HashSet<string> Tags { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="connectionName">コネクション名</param>
        /// <param name="equipmentTypeID">装備種別ID</param>
        /// <param name="groupName">グループ名</param>
        /// <param name="tags">タグ一覧</param>
        public WareEquipment(string wareID, string connectionName, string equipmentTypeID, string groupName, HashSet<string> tags)
        {
            ID = wareID;
            ConnectionName = connectionName;
            EquipmentType = X4Database.Instance.EquipmentType.Get(equipmentTypeID);
            GroupName = groupName;
            Tags = tags;
        }


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
}
