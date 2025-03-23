using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Products;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.Profit;

sealed partial class ProfitModel : ObservableRecipientEx
{
    #region メンバ
    /// <summary>
    /// 製品一覧
    /// </summary>
    private readonly IProductsInfo _products;
    #endregion


    #region プロパティ
    /// <summary>
    /// 利益詳細
    /// </summary>
    public ObservableCollection<ProductsGridItem> ProfitDetails => _products.Products;


    /// <summary>
    /// 利益
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial long Profit { get; set; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="products">製品一覧</param>
    public ProfitModel(IMessenger messenger, IProductsInfo products) : base(messenger, true)
    {
        _products = products;
        _products.Products.CollectionChanged += OnProductsCollectionChanged;
        Messenger.RegisterPropertyChangedMessage(this, static (ProductsGridItem x) => x.Price, static (r, m) => r.Profit -= (m.OldValue - m.NewValue));
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _products.Products.CollectionChanged -= OnProductsCollectionChanged;
        Messenger.UnregisterAll(this);
    }


    /// <summary>
    /// 製品が追加/削除された場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnProductsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // 製品が削除された場合
        if (e.OldItems is not null)
        {
            Profit -= e.OldItems.Cast<ProductsGridItem>().Sum(x => x.Price);
        }

        // 製品が追加された場合
        if (e.NewItems is not null)
        {
            Profit += e.NewItems.Cast<ProductsGridItem>().Sum(x => x.Price);
        }

        // リセットされた場合
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            Profit = _products.Products.Sum(x => x.Price);
        }
    }
}
