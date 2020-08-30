using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StationSettings;
using X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;
using X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.BuildResources;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Products;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StorageAssign;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Storages;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData
{
    /// <summary>
    /// 計算機で使用するステーション用データ
    /// </summary>
    public class StationData : IModulesInfo, IProductsInfo, IBuildResourcesInfo, IStoragesInfo, IStorageAssignInfo
    {
        /// <summary>
        /// モジュール一覧
        /// </summary>
        public ObservablePropertyChangedCollection<ModulesGridItem> Modules { get; } =
            new ObservablePropertyChangedCollection<ModulesGridItem>();


        /// <summary>
        /// 製品一覧
        /// </summary>
        public ObservablePropertyChangedCollection<ProductsGridItem> Products { get; } =
            new ObservablePropertyChangedCollection<ProductsGridItem>();


        /// <summary>
        /// 建造リソース情報
        /// </summary>
        public ObservablePropertyChangedCollection<BuildResourcesGridItem> BuildResources { get; } =
            new ObservablePropertyChangedCollection<BuildResourcesGridItem>();


        /// <summary>
        /// 保管庫情報
        /// </summary>
        public ObservablePropertyChangedCollection<StoragesGridItem> Storages { get; } =
            new ObservablePropertyChangedCollection<StoragesGridItem>();


        /// <summary>
        /// 保管庫割当情報
        /// </summary>
        public ObservablePropertyChangedCollection<StorageAssignGridItem> StorageAssignInfo { get; } =
            new ObservablePropertyChangedCollection<StorageAssignGridItem>();


        /// <summary>
        /// ステーション設定
        /// </summary>
        public IStationSettings Settings = new StationSettings.StationSettings();
    }
}
