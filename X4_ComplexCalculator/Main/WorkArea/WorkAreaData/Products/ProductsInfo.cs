using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Products;

/// <summary>
/// 製品一覧情報用クラス
/// </summary>
public sealed class ProductsInfo
{
    /// <summary>
    /// 製品一覧情報
    /// </summary>
    public ObservableRangeCollection<ProductsGridItem> Products { get; } = [];
}
