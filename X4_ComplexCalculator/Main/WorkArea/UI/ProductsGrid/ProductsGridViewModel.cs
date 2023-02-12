using Prism.Commands;
using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Products;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

/// <summary>
/// 製品一覧用DataGridViewのViewModel
/// </summary>
public sealed class ProductsGridViewModel : BindableBase, IDisposable
{
    #region メンバ
    /// <summary>
    /// 製品一覧用DataGridViewのModel
    /// </summary>
    readonly ProductsGridModel _model;


    /// <summary>
    /// 製品価格割合
    /// </summary>
    private long _unitPricePercent = 50;


    /// <summary>
    /// 不足ウェアを購入しない
    /// </summary>
    private bool _noBuy;


    /// <summary>
    /// 余剰ウェアを売却しない
    /// </summary>
    private bool _noSell;
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
    public double UnitPricePercent
    {
        get => _unitPricePercent;
        set
        {
            _unitPricePercent = (long)value;

            foreach (var product in _model.Products)
            {
                product.SetUnitPricePercent(_unitPricePercent);
            }

            RaisePropertyChanged();
        }
    }


    /// <summary>
    /// 選択されたアイテムの展開/折りたたみ状態を設定する
    /// </summary>
    public ICommand SetSelectedExpandedCommand { get; }


    /// <summary>
    /// モジュール自動追加
    /// </summary>
    public ICommand AutoAddModuleCommand { get; }


    /// <summary>
    /// 選択されたアイテムの不足ウェア購入オプションを設定
    /// </summary>
    public ICommand SetNoBuyToSelectedItemCommand { get; }


    /// <summary>
    /// 選択されたアイテムの余剰ウェア販売オプションを設定
    /// </summary>
    public ICommand SetNoSellToSelectedItemCommand { get; }


    /// <summary>
    /// 不足ウェアを購入しない
    /// </summary>
    public bool NoBuy
    {
        get => _noBuy;
        set
        {
            foreach (var prod in _model.Products)
            {
                prod.NoBuy = value;
            }

            SetProperty(ref _noBuy, value);
        }
    }


    /// <summary>
    /// 余剰ウェアを売却しない
    /// </summary>
    public bool NoSell
    {
        get => _noSell;
        set
        {
            foreach (var prod in _model.Products)
            {
                prod.NoSell = value;
            }

            SetProperty(ref _noSell, value);
        }
    }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="stationData">計算機で使用するステーション情報</param>
    /// <param name="messageBox">メッセージボックス表示用</param>
    public ProductsGridViewModel(IStationData stationData, ILocalizedMessageBox messageBox)
    {
        _model = new ProductsGridModel(stationData.ModulesInfo, stationData.ProductsInfo, stationData.Settings, messageBox);
        ProductsInfo = stationData.ProductsInfo;

        ProductsView = new CollectionViewSource { Source = _model.Products }.View;

        // ソート方向設定
        ProductsView.SortDescriptions.Clear();
        ProductsView.SortDescriptions.Add(new SortDescription("Ware.WareGroup.Tier", ListSortDirection.Ascending));
        ProductsView.SortDescriptions.Add(new SortDescription("Ware.Name", ListSortDirection.Ascending));

        SetSelectedExpandedCommand = new DelegateCommand<bool?>(SetSelectedExpanded);
        AutoAddModuleCommand = new DelegateCommand(_model.AutoAddModule);
        SetNoBuyToSelectedItemCommand = new DelegateCommand<bool?>(SetNoBuyToSelectedItem);
        SetNoSellToSelectedItemCommand = new DelegateCommand<bool?>(SetNoSellToSelectedItem);
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose() => _model.Dispose();


    /// <summary>
    /// 選択されたアイテムの展開/折りたたみ状態を設定
    /// </summary>
    /// <param name="param">展開するか</param>
    private void SetSelectedExpanded(bool? param)
    {
        foreach (var item in _model.Products.Where(x => x.IsSelected))
        {
            item.IsExpanded = param == true;
        }
    }


    /// <summary>
    /// 選択されたアイテムの不足ウェア購入オプションを設定
    /// </summary>
    /// <param name="param">購入しないか</param>
    private void SetNoBuyToSelectedItem(bool? param)
    {
        foreach (var item in _model.Products.Where(x => x.IsSelected))
        {
            item.NoBuy = param == true;
        }
    }


    /// <summary>
    /// 選択されたアイテムの余剰ウェア販売オプションを設定
    /// </summary>
    /// <param name="param">販売しないか</param>
    private void SetNoSellToSelectedItem(bool? param)
    {
        foreach (var item in _model.Products.Where(x => x.IsSelected))
        {
            item.NoSell = param == true;
        }
    }
}
