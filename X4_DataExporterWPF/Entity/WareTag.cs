namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェアのタグ情報
    /// </summary>
    public class WareTag
    {
        /// <summary>
        /// ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// タグ
        /// </summary>
        public string Tag { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="tag">タグ</param>
        public WareTag(string wareID, string tag)
        {
            WareID = wareID;
            Tag = tag;
        }
    }
}
