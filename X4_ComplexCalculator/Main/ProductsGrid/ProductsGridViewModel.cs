using Prism.Commands;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using X4_ComplexCalculator.Common;
using System.Linq;

namespace X4_ComplexCalculator.Main.ProductsGrid
{
    /// <summary>
    /// 製品一覧用DataGridViewのViewModel
    /// </summary>
    class ProductsGridViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 製品一覧用DataGridViewのModel
        /// </summary>
        readonly ProductsGridModel Model;

        /// <summary>
        /// 製品価格割合
        /// </summary>
        private long _UnitPricePercent = 50;
        #endregion


        #region プロパティ
        /// <summary>
        /// 製品一覧
        /// </summary>
        public ObservableCollection<ProductsGridItem> Products => Model.Products;

        /// <summary>
        /// 単価(百分率)
        /// </summary>
        public double UnitPricePercent
        {
            get
            {
                return _UnitPricePercent;
            }
            set
            {
                _UnitPricePercent = (long)value;

                foreach (var product in Products)
                {
                    product.SetUnitPricePercent(_UnitPricePercent);
                }
                
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 選択されたアイテムを展開する
        /// </summary>
        public DelegateCommand<DataGrid> SelectedExpand { get; }

        /// <summary>
        /// 選択されたアイテムを折りたたむ
        /// </summary>
        public DelegateCommand<DataGrid> SelectedCollapse { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="productsGridModel">製品一覧用Model</param>
        public ProductsGridViewModel(ProductsGridModel productsGridModel)
        {
            Model = productsGridModel;
            SelectedExpand = new DelegateCommand<DataGrid>(SelectedExpandCommand);
            SelectedCollapse = new DelegateCommand<DataGrid>(SelectedCollapseCommand);
        }

        /// <summary>
        /// 選択されたアイテムを展開する
        /// </summary>
        /// <param name="dataGrid"></param>
        private void SelectedExpandCommand(DataGrid dataGrid)
        {
            foreach(ProductsGridItem item in dataGrid.SelectedCells.Select(x => x.Item))
            {
                item.IsExpanded = true;
            }
        }

        /// <summary>
        /// 選択されたアイテムを折りたたむ
        /// </summary>
        /// <param name="dataGrid"></param>
        private void SelectedCollapseCommand(DataGrid dataGrid)
        {
            foreach (ProductsGridItem item in dataGrid.SelectedCells.Select(x => x.Item))
            {
                item.IsExpanded = false;
            }
        }
    }
}
