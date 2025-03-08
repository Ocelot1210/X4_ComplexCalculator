using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.View.EmpireOverviews;

/// <summary>
/// 帝国の概要用ViewModel
/// </summary>
sealed partial class EmpireOverviewWindowViewModel : ObservableObject
{
    #region メンバ
    /// <summary>
    /// 帝国の概要用Model
    /// </summary>
    private readonly EmpireOverviewWindowModel _model;
    #endregion


    #region プロパティ
    /// <summary>
    /// 製品一覧
    /// </summary>
    public ICollectionView ProductsView { get; }


    /// <summary>
    /// 計画一覧
    /// </summary>
    public ICollectionView WorkAreasView { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="workAreas">帝国の概要用Model</param>
    public EmpireOverviewWindowViewModel(ObservableCollection<WorkAreaViewModel> workAreas)
    {
        _model = new EmpireOverviewWindowModel(workAreas);
        ProductsView = CollectionViewSource.GetDefaultView(_model.Products);
        ProductsView.Filter = Filter;
        ProductsView.SortDescriptions.Add(new SortDescription("Ware.WareGroup.Tier", ListSortDirection.Ascending));
        ProductsView.SortDescriptions.Add(new SortDescription("Ware.Name", ListSortDirection.Ascending));

        WorkAreasView = CollectionViewSource.GetDefaultView(_model.WorkAreas);
        WorkAreasView.SortDescriptions.Add(new SortDescription(nameof(WorkAreaItem.Title), ListSortDirection.Ascending));
    }


    /// <summary>
    /// フィルタイベント
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private bool Filter(object obj)
    {
        return obj is EmpireOverViewProductsGridItem item && (item.Surplus != 0 || item.Shortage != 0);
    }


    /// <summary>
    /// ウィンドウが閉じられた時
    /// </summary>
    [RelayCommand]
    private void OnWindowClosed() => _model.Dispose();
}
