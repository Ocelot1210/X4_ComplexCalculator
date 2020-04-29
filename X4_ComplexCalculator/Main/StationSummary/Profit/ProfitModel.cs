using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.ProductsGrid;
using System.ComponentModel;

namespace X4_ComplexCalculator.Main.StationSummary.Profit
{
    class ProfitModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 製品一覧
        /// </summary>
        private readonly ObservablePropertyChangedCollection<ProductsGridItem> Products;

        /// <summary>
        /// 利益
        /// </summary>
        private long _Profit = 0;
        #endregion


        #region プロパティ
        /// <summary>
        /// 利益詳細
        /// </summary>
        public ObservableRangeCollection<ProfitDetailsItem> ProfitDetails { get; } = new ObservableRangeCollection<ProfitDetailsItem>();


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
        public ProfitModel(ObservablePropertyChangedCollection<ProductsGridItem> products)
        {
            Products = products;
            Products.CollectionChangedAsync += OnProductsCollectionChanged;
            Products.CollectionPropertyChangedAsync += OnProductsPropertyChanged;
        }

        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            Products.CollectionChangedAsync -= OnProductsCollectionChanged;
            Products.CollectionPropertyChangedAsync -= OnProductsPropertyChanged;
            ProfitDetails.Clear();
        }

        /// <summary>
        /// 製品のプロパティが変更された時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnProductsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateProfit();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 製品一覧が変更された時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnProductsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "UnitPrice")
            {
                return;
            }

            if (!(sender is ProductsGridItem product))
            {
                return;
            }

            var item = ProfitDetails.Where(x => x.WareID == product.Ware.WareID).First();
            Profit = Profit - item.TotalPrice + product.Price;
            item.UnitPrice = product.UnitPrice;

            await Task.CompletedTask;
        }


        /// <summary>
        /// 利益を更新
        /// </summary>
        private void UpdateProfit()
        {
            var profit = 0L;
            var items = Products.Select(x =>
            {
                var ret = new ProfitDetailsItem(x.Ware.WareID, x.Ware.Name, x.Count, x.UnitPrice);
                profit += ret.TotalPrice;
                return ret;
            });

            ProfitDetails.Reset(items);
            Profit = profit;
        }
    }
}
