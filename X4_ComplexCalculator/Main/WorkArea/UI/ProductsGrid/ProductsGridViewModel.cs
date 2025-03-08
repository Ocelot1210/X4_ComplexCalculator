using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.ComponentModel;
using System.Windows.Data;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Products;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

/// <summary>
/// 製品一覧用DataGridViewのViewModel
/// </summary>
public sealed partial class ProductsGridViewModel : ObservableObject, IDisposable
{
    #region メンバ
    /// <summary>
    /// 製品一覧用DataGridViewのModel
    /// </summary>
    readonly ProductsGridModel _model;
    #endregion


    #region プロパティ
    /// <summary>
    /// 製品一覧
    /// </summary>
    public ICollectionView ProductsView { get; }


    /// <summary>
    /// 製品情報
    /// </summary>
    public IProductsInfo ProductsInfo { get; }


    /// <summary>
    /// 単価(百分率)
    /// </summary>
    [ObservableProperty]
    public partial double UnitPricePercent { get; set; } = 50;


    /// <summary>
    /// 不足ウェアを購入しない
    /// </summary>
    [ObservableProperty]
    public partial bool NoBuy { get; set; }


    /// <summary>
    /// 余剰ウェアを売却しない
    /// </summary>
    [ObservableProperty]
    public partial bool NoSell { get; set; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messenger">メッセージ通知用</param>
    /// <param name="stationData">計算機で使用するステーション情報</param>
    public ProductsGridViewModel(IMessenger messenger, IStationData stationData)
    {
        _model = new ProductsGridModel(messenger, stationData.ModulesInfo, stationData.ProductsInfo, stationData.Settings);
        ProductsInfo = stationData.ProductsInfo;

        ProductsView = new CollectionViewSource { Source = _model.Products }.View;

        // ソート方向設定
        ProductsView.SortDescriptions.Clear();
        ProductsView.SortDescriptions.Add(new SortDescription("Ware.WareGroup.Tier", ListSortDirection.Ascending));
        ProductsView.SortDescriptions.Add(new SortDescription("Ware.Name", ListSortDirection.Ascending));
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose() => _model.Dispose();


    /// <summary>
    /// 価格を一括設定
    /// </summary>
    /// <param name="value"></param>
    partial void OnUnitPricePercentChanged(double value) => _model.SetUnitPricePercent((long)value);


    /// <summary>
    /// 購入フラグを一括設定
    /// </summary>
    /// <param name="value">設定値</param>
    partial void OnNoBuyChanged(bool value) => _model.SetNoBuy(value, false);


    /// <summary>
    /// 販売フラグを一括設定
    /// </summary>
    /// <param name="value">設定値</param>
    partial void OnNoSellChanged(bool value) => _model.SetNoSell(value, false);


    /// <summary>
    /// 選択されたアイテムの展開/折りたたみ状態を設定
    /// </summary>
    /// <param name="param">展開するか</param>
    [RelayCommand]
    private void SetSelectedExpanded(bool? param) => _model.SetExpanded(param == true);


    /// <summary>
    /// 選択されたアイテムの不足ウェア購入オプションを設定
    /// </summary>
    /// <param name="param">購入しないか</param>
    [RelayCommand]
    private void SetNoBuyToSelectedItem(bool? param) => _model.SetNoBuy(param == true, true);


    /// <summary>
    /// 選択されたアイテムの余剰ウェア販売オプションを設定
    /// </summary>
    /// <param name="param">販売しないか</param>
    [RelayCommand]
    private void SetNoSellToSelectedItem(bool? param) => _model.SetNoSell(param == true, true);
}
