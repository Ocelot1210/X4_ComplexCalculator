namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 用途
    /// </summary>
    public class Purpose
    {
        /// <summary>
        /// 用途ID
        /// </summary>
        public string PurposeID { get; }


        /// <summary>
        /// 用途名称
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="purposeID">用途ID</param>
        /// <param name="name">用途名称</param>
        public Purpose(string purposeID, string name)
        {
            PurposeID = purposeID;
            Name = name;
        }
    }
}
