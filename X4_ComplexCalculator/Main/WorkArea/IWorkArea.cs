using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.Menu.File.Export;
using X4_ComplexCalculator.Main.Menu.File.Import;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.WorkArea
{
    interface IWorkArea
    {
        /// <summary>
        /// モジュール一覧
        /// </summary>
        public ObservableRangeCollection<ModulesGridItem> Modules { get; }

        /// <summary>
        /// 製品一覧
        /// </summary>
        public ObservableRangeCollection<ProductsGridItem> Products { get; }

        /// <summary>
        /// 建造リソース一覧
        /// </summary>
        public ObservableRangeCollection<ResourcesGridItem> Resources { get; }

        /// <summary>
        /// 保管庫割当情報
        /// </summary>
        public ObservableRangeCollection<StorageAssignGridItem> StorageAssign { get; }
    }
}
