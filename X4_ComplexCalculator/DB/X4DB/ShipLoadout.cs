using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 艦船のロードアウト情報
    /// </summary>
    public class ShipLoadout
    {
        #region メンバ
        /// <summary>
        /// 装備品ID
        /// </summary>
        private readonly string _EquipmentID;
        #endregion


        #region プロパティ
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ID { get; }


        /// <summary>
        /// ロードアウトID
        /// </summary>
        public string LoadoutID { get; }


        /// <summary>
        /// 装備品
        /// </summary>
        public IEquipment Equipment => X4Database.Instance.Ware.Get<IEquipment>(_EquipmentID);


        /// <summary>
        /// グループ名
        /// </summary>
        public string GroupName { get; }


        /// <summary>
        /// 個数
        /// </summary>
        public long Count { get; }
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <param name="loadoutID">ロードアウトID</param>
        /// <param name="groupName">グループ名</param>
        /// <param name="count">個数</param>
        /// <param name="equipmentID">装備ID</param>
        public ShipLoadout(string shipID, string loadoutID, string groupName, long count, string equipmentID)
        {
            ID = shipID;
            LoadoutID = loadoutID;
            _EquipmentID = equipmentID;
            GroupName = groupName;
            Count = count;
        }
    }
}
