﻿using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Products;

/// <summary>
/// 製品一覧情報用インターフェイス
/// </summary>
public interface IProductsInfo
{
    /// <summary>
    /// 製品一覧情報
    /// </summary>
    public ObservablePropertyChangedCollection<ProductsGridItem> Products { get; }
}
