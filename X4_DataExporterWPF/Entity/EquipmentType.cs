namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 装備種別
    /// </summary>
    public class EquipmentType
    {
        #region プロパティ
        /// <summary>
        /// 装備種別ID
        /// </summary>
        public string EquipmentTypeID { get; }


        /// <summary>
        /// 装備種別名
        /// </summary>
        public string Name { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentTypeID">装備種別ID</param>
        /// <param name="name">装備種別名</param>
        public EquipmentType(string equipmentTypeID, string name)
        {
            EquipmentTypeID = equipmentTypeID;
            Name = name;
        }
    }
}
