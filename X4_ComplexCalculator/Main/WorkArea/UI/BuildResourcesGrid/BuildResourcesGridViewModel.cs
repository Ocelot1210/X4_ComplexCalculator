using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;

namespace X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;

/// <summary>
/// 建造に必要なリソースを表示するDataGridView用ViewModel
/// </summary>
public sealed class BuildResourcesGridViewModel : BindableBase, IDisposable
{
    #region メンバ
    /// <summary>
    /// 建造に必要なリソースを表示するDataGridView用Model
    /// </summary>
    private readonly BuildResourcesGridModel _model;


    /// <summary>
    /// 価格割合
    /// </summary>
    private long _unitPricePercent = 50;


    /// <summary>
    /// 建造に必要なウェアを購入しない
    /// </summary>
    private bool _noBuy;
    #endregion


    #region プロパティ
    /// <summary>
    /// 建造に必要なリソース一覧
    /// </summary>
    public ObservableCollection<BuildResourcesGridItem> BuildResource => _model.Resources;


    /// <summary>
    /// 建造に必要なリソース一覧
    /// </summary>
    public ICollectionView BuildResourceView { get; }


    /// <summary>
    /// 選択されたアイテムの建造に必要なウェア購入オプションを設定
    /// </summary>
    public ICommand SetNoBuyToSelectedItemCommand { get; }


    /// <summary>
    /// 単価(百分率)
    /// </summary>
    public double UnitPricePercent
    {
        get => _unitPricePercent;
        set
        {
            _unitPricePercent = (long)value;

            foreach (var resource in BuildResource)
            {
                resource.SetUnitPricePercent(_unitPricePercent);
            }

            RaisePropertyChanged();
        }
    }


    /// <summary>
    /// 建造に必要なウェアを購入しない
    /// </summary>
    public bool NoBuy
    {
        get => _noBuy;
        set
        {
            if (SetProperty(ref _noBuy, value))
            {
                foreach (var ware in BuildResource)
                {
                    ware.NoBuy = value;
                }
            }
        }
    }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="stationData">計算機で使用するステーション情報</param>
    public BuildResourcesGridViewModel(IStationData stationData)
    {
        _model = new BuildResourcesGridModel(stationData.ModulesInfo, stationData.BuildResourcesInfo);

        BuildResourceView = new CollectionViewSource { Source = _model.Resources }.View;
        BuildResourceView.SortDescriptions.Add(new SortDescription("Ware.Name", ListSortDirection.Ascending));
        SetNoBuyToSelectedItemCommand = new DelegateCommand<bool?>(SetNoBuyToSelectedItem);
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _model.Dispose();
    }


    /// <summary>
    /// 選択されたアイテムの建造に必要なウェア購入オプションを設定
    /// </summary>
    /// <param name="param"></param>
    private void SetNoBuyToSelectedItem(bool? param)
    {
        foreach (var item in _model.Resources.Where(x => x.IsSelected))
        {
            item.NoBuy = param == true;
        }
    }
}
