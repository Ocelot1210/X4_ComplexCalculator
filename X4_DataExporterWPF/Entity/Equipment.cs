namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 装備
    /// </summary>
    public class Equipment
    {
        #region プロパティ
        /// <summary>
        /// 装備ID
        /// </summary>
        public string EquipmentID { get; }


        /// <summary>
        /// マクロ名
        /// </summary>
        public string MacroName { get; }


        /// <summary>
        /// 装備種別ID
        /// </summary>
        public string EquipmentTypeID { get; }


        /// <summary>
        /// サイズID
        /// </summary>
        public string SizeID { get; }


        /// <summary>
        /// 装備名
        /// </summary>
        public string Name { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <param name="macroName">マクロ名</param>
        /// <param name="equipmentTypeID">装備種別ID</param>
        /// <param name="sizeID">サイズID</param>
        /// <param name="name">装備名</param>
        public Equipment(string equipmentID, string macroName, string equipmentTypeID, string sizeID, string name)
        {
            EquipmentID = equipmentID;
            MacroName = macroName;
            EquipmentTypeID = equipmentTypeID;
            SizeID = sizeID;
            Name = name;
        }
    }
}
