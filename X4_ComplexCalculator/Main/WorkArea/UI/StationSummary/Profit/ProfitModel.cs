using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.Products;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.Profit
{
    class ProfitModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 製品一覧
        /// </summary>
        private readonly IProductsInfo _Products;

        /// <summary>
        /// 利益
        /// </summary>
        private long _Profit = 0;
        #endregion


        #region プロパティ
        /// <summary>
        /// 利益詳細
        /// </summary>
        public ObservableCollection<ProductsGridItem> ProfitDetails => _Products.Products;


        /// <summary>
        /// 利益
        /// </summary>
        public long Profit
        {
            get => _Profit;
            set => SetProperty(ref _Profit, value);
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="products">製品一覧</param>
        public ProfitModel(IProductsInfo products)
        {
            _Products = products;
            _Products.Products.CollectionChanged += OnProductsCollectionChanged;
            _Products.Products.CollectionPropertyChanged += OnProductsPropertyChanged;
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _Products.Products.CollectionChanged -= OnProductsCollectionChanged;
            _Products.Products.CollectionPropertyChanged -= OnProductsPropertyChanged;
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
                Profit = _Products.Products.Sum(x => x.Price);
            }
        }


        /// <summary>
        /// 製品のプロパティが変更された時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProductsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // 価格変更の場合
                case nameof(ProductsGridItem.Price):
                    if (e is PropertyChangedExtendedEventArgs<long> ev)
                    {
                        Profit -= (ev.OldValue - ev.NewValue);
                    }
                    break;

                // それ以外の場合
                default:
                    break;
            }
        }
    }
}
