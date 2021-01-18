namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェアの装備情報
    /// </summary>
    public class WareEquipment
    {
        /// <summary>
        /// ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// コネクション名
        /// </summary>
        public string ConnectionName { get; }


        /// <summary>
        /// 装備種別
        /// </summary>
        public string EquipmentTypeID { get; }


        /// <summary>
        /// グループ名
        /// </summary>
        public string? GroupName { get; }



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="connectionName">コネクション名</param>
        /// <param name="equipmentTypeID">装備種別</param>
        /// <param name="groupName">グループ名</param>
        public WareEquipment(string wareID, string connectionName, string equipmentTypeID, string? groupName)
        {
            WareID = wareID;
            ConnectionName = connectionName;
            EquipmentTypeID = equipmentTypeID;
            GroupName = groupName;
        }
    }
}
