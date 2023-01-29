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
        ProductsView.SortDescriptions.Add(new SortDescription("Ware.WareGroup.Tier", ListSortDirection.Ascending));
        ProductsView.SortDescriptions.Add(new SortDescription("Ware.Name", ListSortDirection.Ascending));

        WindowClosedCommand = new DelegateCommand(WindowClosed);
    }


    /// <summary>
    /// ウィンドウが閉じられた時
    /// </summary>
    private void WindowClosed()
    {
        _model.Dispose();
    }
}
