namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船の装備情報
    /// </summary>
    public class ShipEquipment
    {
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ShipID { get; }


        /// <summary>
        /// 装備種別ID
        /// </summary>
        public string EquipmentTypeID { get; }


        /// <summary>
        /// 装備のサイズID
        /// </summary>
        public string SizeID { get; }


        /// <summary>
        /// 装備可能な数
        /// </summary>
        public int Count { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <param name="equipmentTypeID">装備種別ID</param>
        /// <param name="sizeID">サイズID</param>
        /// <param name="count">装備可能な個数</param>
        public ShipEquipment(string shipID, string equipmentTypeID, string sizeID, int count)
        {
            ShipID = shipID;
            EquipmentTypeID = equipmentTypeID;
            SizeID = sizeID;
            Count = count;
        }
    }
}
