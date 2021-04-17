namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// 選択可能アイテム用インターフェイス
    /// </summary>
    /// <remarks>
    /// DataGrid等のItemsSourceに設定するアイテムが継承する事を想定
    /// </remarks>
    public interface ISelectable
    {
        /// <summary>
        /// 選択されているか
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
