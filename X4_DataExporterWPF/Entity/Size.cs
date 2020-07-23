namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// サイズ
    /// </summary>
    public class Size
    {
        #region プロパティ
        /// <summary>
        /// サイズID
        /// </summary>
        public string SizeID { get; }


        /// <summary>
        /// サイズ名
        /// </summary>
        public string Name { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sizeID">サイズID</param>
        /// <param name="name">サイズ名</param>
        public Size(string sizeID, string name)
        {
            SizeID = sizeID;
            Name = name;
        }
    }
}
