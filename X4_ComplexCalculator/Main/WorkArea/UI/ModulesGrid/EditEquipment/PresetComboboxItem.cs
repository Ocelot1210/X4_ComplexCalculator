namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment
{
    /// <summary>
    /// プリセットコンボボックス用アイテム
    /// </summary>
    class PresetComboboxItem
    {
        /// <summary>
        /// プリセットID
        /// </summary>
        public long ID { get; }

        /// <summary>
        /// プリセット名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">プリセットID(内部用)</param>
        /// <param name="name">プリセット名</param>
        public PresetComboboxItem(long id, string name)
        {
            ID = id;
            Name = name;
        }
    }
}
