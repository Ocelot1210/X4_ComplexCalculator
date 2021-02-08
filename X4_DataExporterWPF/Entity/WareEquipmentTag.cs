namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェアの装備のタグ情報
    /// </summary>
    public class WareEquipmentTag
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
        /// タグの値
        /// </summary>
        public string Tag { get; }


        /// <summary>
        /// コンストラクタ 
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="connectionName">コネクション名</param>
        /// <param name="tag">タグの値</param>
        public WareEquipmentTag(string wareID, string connectionName, string tag)
        {
            WareID = wareID;
            ConnectionName = connectionName;
            Tag = tag;
        }
    }
}
