using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main.Menu.View.EmpireOverview;

/// <summary>
/// 帝国の概要用ViewModel
/// </summary>
class EmpireOverviewWindowViewModel : BindableBase
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


    /// <summary>
    /// ウィンドウが閉じられた時のコマンド
    /// </summary>
    public ICommand WindowClosedCommand { get; }
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

        WindowClosedCommand = new DelegateCommand(WindowClosed);
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
    private void WindowClosed()
    {
        _model.Dispose();
    }
}
