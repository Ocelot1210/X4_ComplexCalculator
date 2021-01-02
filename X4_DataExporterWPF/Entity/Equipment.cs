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


        /// <summary>
        /// 船体値
        /// </summary>
        public int Hull { get; }


        /// <summary>
        /// 船体値が統合されているか
        /// </summary>
        public bool HullIntegrated { get; }


        /// <summary>
        /// Mk
        /// </summary>
        public int Mk { get; }


        /// <summary>
        /// 作成種族
        /// </summary>
        public string? MakerRace { get; }


        /// <summary>
        /// 説明文
        /// </summary>
        public string Description { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <param name="macroName">マクロ名</param>
        /// <param name="equipmentTypeID">装備種別ID</param>
        /// <param name="sizeID">サイズID</param>
        /// <param name="name">装備名</param>
        /// <param name="mk">Mk</param>
        /// <param name="makerRace">作成種族</param>
        /// <param name="description">説明文</param>
        public Equipment(
            string equipmentID,
            string macroName,
            string equipmentTypeID,
            string sizeID,
            string name,
            int hull,
            bool hullIntegrated,
            int mk,
            string? makerRace,
            string description)
        {
            EquipmentID = equipmentID;
            MacroName = macroName;
            EquipmentTypeID = equipmentTypeID;
            SizeID = sizeID;
            Name = name;
            Hull = hull;
            HullIntegrated = hullIntegrated;
            Mk = mk;
            MakerRace = makerRace;
            Description = description;
        }
    }
}
