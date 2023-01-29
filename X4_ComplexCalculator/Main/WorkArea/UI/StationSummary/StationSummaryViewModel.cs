using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.BuildingCost;
using X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.Profit;
using X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.ModuleInfo;
using X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.NeedWareInfo;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary;

/// <summary>
/// ステーション概要用ViewModel
/// </summary>
public sealed class StationSummaryViewModel : BindableBase, IDisposable
{
    #region メンバ
    /// <summary>
    /// 労働力用Model
    /// </summary>
    private readonly WorkForceModuleInfoModel _workForceModuleInfoModel;


    /// <summary>
    /// 必要ウェア用Model
    /// </summary>
    private readonly NeedWareInfoModel _needWareInfoModel;


    /// <summary>
    /// 利益用Model
    /// </summary>
    private readonly ProfitModel _profitModel;


    /// <summary>
    /// 建造コスト用Model
    /// </summary>
    private readonly BuildingCostModel _buildingCostModel;
    #endregion


    #region 労働力関連プロパティ
    /// <summary>
    /// 労働者管理用
    /// </summary>
    public WorkforceManager Workforce { get; }


    /// <summary>
    /// 労働力関連モジュール情報
    /// </summary>
    public ObservableCollection<WorkForceModuleInfoDetailsItem> WorkforceModuleDetails => _workForceModuleInfoModel.WorkForceDetails;


    /// <summary>
    /// 必要ウェア情報
    /// </summary>
    public ListCollectionView WorkforceNeedWareCollectionView { get; }
    #endregion


    #region 損益関連プロパティ
    /// <summary>
    /// 1時間あたりの損益
    /// </summary>
    public long Profit => _profitModel.Profit;


    /// <summary>
    /// 損益詳細
    /// </summary>
    public ObservableCollection<ProductsGridItem> ProfitDetails => _profitModel.ProfitDetails;
    #endregion


    #region 建造コスト関連プロパティ
    /// <summary>
    /// 建造費用
    /// </summary>
    public long BuildingCost => _buildingCostModel.BuildingCost;


    /// <summary>
    /// 建造コスト詳細
    /// </summary>
    public ObservableCollection<BuildResourcesGridItem> BuildingCostDetails => _buildingCostModel.BuildResources;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="stationData">計算機で使用するステーション情報</param>
    public StationSummaryViewModel(IStationData stationData)
    {
        Workforce = stationData.Settings.Workforce;

        // 労働力関係初期化
        {
            _workForceModuleInfoModel = new WorkForceModuleInfoModel(stationData.ModulesInfo, stationData.Settings);
        }

        {
            _needWareInfoModel = new NeedWareInfoModel(stationData.ModulesInfo, stationData.ProductsInfo);

            WorkforceNeedWareCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(_needWareInfoModel.NeedWareInfoDetails);
            WorkforceNeedWareCollectionView.SortDescriptions.Clear();
            WorkforceNeedWareCollectionView.SortDescriptions.Add(new SortDescription(nameof(NeedWareInfoDetailsItem.Method), ListSortDirection.Ascending));
            WorkforceNeedWareCollectionView.SortDescriptions.Add(new SortDescription(nameof(NeedWareInfoDetailsItem.WareName), ListSortDirection.Ascending));
            WorkforceNeedWareCollectionView.GroupDescriptions.Clear();

            WorkforceNeedWareCollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(NeedWareInfoDetailsItem.Method)));
        }


        // 損益関係初期化
        {
            _profitModel = new ProfitModel(stationData.ProductsInfo);
            _profitModel.PropertyChanged += ProfitModel_PropertyChanged;
        }


        // 建造コスト関係初期化
        {
            _buildingCostModel = new BuildingCostModel(stationData.BuildResourcesInfo);
            _buildingCostModel.PropertyChanged += BuildingCostModel_PropertyChanged;
        }
    }


    /// <summary>
    /// 損益情報用Modelのプロパティ変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ProfitModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ProfitModel.Profit):
                RaisePropertyChanged(nameof(Profit));
                break;

            default:
                break;
        }
    }


    /// <summary>
    /// 建造コスト用Modelのプロパティ変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BuildingCostModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(BuildingCostModel.BuildingCost):
                RaisePropertyChanged(nameof(BuildingCost));
                break;

            default:
                break;
        }
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _profitModel.PropertyChanged       -= ProfitModel_PropertyChanged;
        _buildingCostModel.PropertyChanged -= BuildingCostModel_PropertyChanged;

        _workForceModuleInfoModel.Dispose();
        _needWareInfoModel.Dispose();
        _profitModel.Dispose();
        _buildingCostModel.Dispose();
    }
}
