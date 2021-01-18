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
        /// サムネ画像
        /// </summary>
        public byte[]? Thumbnail { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <param name="macroName">マクロ名</param>
        /// <param name="equipmentTypeID">装備種別ID</param>
        /// <param name="hull"></param>
        /// <param name="hullIntegrated"></param>
        /// <param name="mk">Mk</param>
        /// <param name="makerRace">作成種族</param>
        /// <param name="thumbnail">サムネ画像</param>
        public Equipment(
            string equipmentID,
            string macroName,
            string equipmentTypeID,
            int hull,
            bool hullIntegrated,
            int mk,
            string? makerRace,
            byte[]? thumbnail
            )
        {
            EquipmentID = equipmentID;
            MacroName = macroName;
            EquipmentTypeID = equipmentTypeID;
            Hull = hull;
            HullIntegrated = hullIntegrated;
            Mk = mk;
            MakerRace = makerRace;
            Thumbnail = thumbnail;
        }
    }
}
