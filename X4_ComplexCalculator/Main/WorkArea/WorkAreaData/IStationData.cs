using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.BuildResources;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Modules;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Products;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StorageAssign;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Storages;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData;

/// <summary>
/// 計算機で使用するステーション用データ
/// </summary>
public interface IStationData
{
    /// <summary>
    /// モジュール一覧
    /// </summary>
    IBuildResourcesInfo BuildResourcesInfo { get; }


    /// <summary>
    /// 製品情報
    /// </summary>
    IModulesInfo ModulesInfo { get; }


    /// <summary>
    /// 建造リソース情報
    /// </summary>
    IProductsInfo ProductsInfo { get; }


    /// <summary>
    /// 保管庫情報
    /// </summary>
    IStationSettings Settings { get; }


    /// <summary>
    /// 保管庫割当情報
    /// </summary>
    IStorageAssignInfo StorageAssignInfo { get; }


    /// <summary>
    /// ステーション設定
    /// </summary>
    IStoragesInfo StoragesInfo { get; }
}