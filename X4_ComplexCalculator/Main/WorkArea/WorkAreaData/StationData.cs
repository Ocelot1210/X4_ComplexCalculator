using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.BuildResources;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Products;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StorageAssign;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Storages;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData;

/// <summary>
/// 計算機で使用するステーション用データ用クラス
/// </summary>
public class StationData : IStationData
{
    /// <summary>
    /// モジュール一覧
    /// </summary>
    public IModulesInfo ModulesInfo { get; } = new ModulesInfo();


    /// <summary>
    /// 製品情報
    /// </summary>
    public IProductsInfo ProductsInfo { get; } = new ProductsInfo();


    /// <summary>
    /// 建造リソース情報
    /// </summary>
    public IBuildResourcesInfo BuildResourcesInfo { get; } = new BuildResourcesInfo();


    /// <summary>
    /// 保管庫情報
    /// </summary>
    public IStoragesInfo StoragesInfo { get; } = new StoragesInfo();


    /// <summary>
    /// 保管庫割当情報
    /// </summary>
    public IStorageAssignInfo StorageAssignInfo { get; } = new StorageAssignInfo();


    /// <summary>
    /// ステーション設定
    /// </summary>
    public IStationSettings Settings { get; } = new StationSettings.StationSettings();
}
