namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// セクターマップ情報
    /// </summary>
    public class Map
    {
        #region プロパティ
        /// <summary>
        /// マクロ名
        /// </summary>
        public string Macro { get; }


        /// <summary>
        /// セクター名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 説明
        /// </summary>
        public string Description { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="macro">マクロ名</param>
        /// <param name="name">セクター名</param>
        /// <param name="description">説明</param>
        public Map(string macro, string name, string description)
        {
            Macro = macro;
            Name = name;
            Description = description;
        }
    }
}
