namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// データベースメタデータ
    /// </summary>
    public class Common
    {
        #region プロパティ
        /// <summary>
        /// 項目名
        /// </summary>
        public string Item { get; }


        /// <summary>
        /// 値
        /// </summary>
        public long Value { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="item">項目名</param>
        /// <param name="value">値</param>
        public Common(string item, long value)
        {
            Item = item;
            Value = value;
        }
    }
}
