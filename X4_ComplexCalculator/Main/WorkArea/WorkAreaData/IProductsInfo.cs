using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData
{
    /// <summary>
    /// 製品一覧情報用インターフェイス
    /// </summary>
    interface IProductsInfo
    {
        /// <summary>
        /// 製品一覧情報
        /// </summary>
        public ObservablePropertyChangedCollection<ProductsGridItem> Products { get; }
    }
}
