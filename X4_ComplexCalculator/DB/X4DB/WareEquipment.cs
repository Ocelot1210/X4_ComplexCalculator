using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// ウェアの装備情報
    /// </summary>
    public class WareEquipment
    {
        #region スタティックメンバ
        /// <summary>
        /// ダミー用ウェアの装備情報一覧
        /// </summary>
        private static readonly IReadOnlyList<WareEquipment> _DummyEquipments = Array.Empty<WareEquipment>();


        /// <summary>
        /// ウェアの装備情報一覧
        /// </summary>
        private static readonly Dictionary<string, IReadOnlyList<WareEquipment>> _WareEquipments = new();


        /// <summary>
        /// タグ一覧
        /// </summary>

        private static readonly Dictionary<string, HashSet<string>> _TagsDict = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// ウェアID
        /// </summary>
        public string ID { get; }


        /// <summary>
        /// コネクション名
        /// </summary>
        public string ConnectionName { get; }


        /// <summary>
        /// 装備種別
        /// </summary>
        public EquipmentType EquipmentType { get; }


        /// <summary>
        /// グループ名
        /// </summary>
        public string GroupName { get; }


        /// <summary>
        /// タグ情報
        /// </summary>
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


        /// <summary>
        /// ウェアの装備情報を取得する
        /// </summary>
        /// <param name="id">ウェアID</param>
        /// <returns>ウェアIDに対応するウェアの装備情報一覧</returns>
        public static IReadOnlyList<WareEquipment> Get(string id) => _WareEquipments.TryGetValue(id, out var value) ? value : _DummyEquipments;



        /// <summary>
        /// 指定した装備がthisに装備可能か判定する
        /// </summary>
        /// <param name="equipment">判定したい装備</param>
        /// <returns>指定した装備がthisに装備可能か</returns>
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
