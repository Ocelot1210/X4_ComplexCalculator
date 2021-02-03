namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 装備のタグ情報
    /// </summary>
    public class EquipmentTag
    {
        /// <summary>
        /// 装備ID
        /// </summary>
        public string EquipmentID { get; }


        /// <summary>
        /// タグ
        /// </summary>
        public string Tag { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <param name="tag">タグ</param>
        public EquipmentTag(string equipmentID, string tag)
        {
            EquipmentID = equipmentID;
            Tag = tag;
        }
    }
}
