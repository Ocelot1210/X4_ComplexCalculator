using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.ComponentModel;
using System.Windows.Data;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;

namespace X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;

/// <summary>
/// 建造に必要なリソースを表示するDataGridView用ViewModel
/// </summary>
public sealed partial class BuildResourcesGridViewModel : ObservableRecipient, IDisposable
{
    #region メンバ
    /// <summary>
    /// 建造に必要なリソースを表示するDataGridView用Model
    /// </summary>
    private readonly BuildResourcesGridModel _model;
    #endregion


    #region プロパティ
    /// <summary>
    /// 建造に必要なリソース一覧
    /// </summary>
    public ICollectionView BuildResourceView { get; }


    /// <summary>
    /// 単価(百分率)
    /// </summary>
    [ObservableProperty]
    public partial double UnitPricePercent { get; set; } = 50;


    /// <summary>
    /// 建造に必要なウェアを購入しない
    /// </summary>
    [ObservableProperty]
    public partial bool NoBuy { get; set; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messanger">メッセージ交換用</param>
    /// <param name="stationData">計算機で使用するステーション情報</param>
    public BuildResourcesGridViewModel(IMessenger messenger, StationData stationData) : base(messenger)
    {
        _model = new BuildResourcesGridModel(messenger, stationData.ModulesInfo, stationData.BuildResourcesInfo);

        BuildResourceView = new CollectionViewSource { Source = _model.Resources }.View;
        BuildResourceView.SortDescriptions.Add(new SortDescription("Ware.Name", ListSortDirection.Ascending));
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _model.Dispose();
    }


    /// <summary>
    /// 価格割合一括設定
    /// </summary>
    /// <param name="value">設定値</param>
    partial void OnUnitPricePercentChanging(double value) => _model.SetUnitPricePercent((long)value);


    /// <summary>
    /// 購入しないチェック一括設定
    /// </summary>
    /// <param name="value">設定値</param>
    partial void OnNoBuyChanged(bool value) => _model.SetNoBuy(value, false);


    /// <summary>
    /// 選択されたアイテムの建造に必要なウェア購入オプションを設定
    /// </summary>
    /// <param name="param">設定値</param>
    [RelayCommand]
    private void SetNoBuyToSelectedItem(bool? param) => _model.SetNoBuy(param == true, true);
}
