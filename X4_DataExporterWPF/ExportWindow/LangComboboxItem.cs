namespace X4_DataExporterWPF.DataExportWindow
{
    /// <summary>
    /// 言語コンボボックス用アイテム
    /// </summary>
    class LangComboboxItem
    {
        /// <summary>
        /// 言語ID
        /// </summary>
        public int ID { get; }


        /// <summary>
        /// 言語名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">言語ID</param>
        /// <param name="name">言語名</param>
        public LangComboboxItem(int id, string name)
        {
            ID = id;
            Name = name;
        }
    }
}
