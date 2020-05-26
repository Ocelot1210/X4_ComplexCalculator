using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.PlanningArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.PlanningArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.PlanningArea.UI.ResourcesGrid;
using X4_ComplexCalculator.Main.PlanningArea.UI.StorageAssign;

namespace X4_ComplexCalculator.Main.PlanningArea
{
    interface IPlanningArea
    {
        /// <summary>
        /// タイトル文字列
        /// </summary>
        public string Title { get; set; }

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
