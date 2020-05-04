using Prism.Commands;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using X4_ComplexCalculator.Common;
using System.Linq;
using System.Windows.Input;
using System.Windows.Data;
using System.ComponentModel;
using X4_ComplexCalculator.Main.WorkArea.ModulesGrid;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.ProductsGrid
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
        readonly ProductsGridModel _Model;

        /// <summary>
        /// 製品価格割合
        /// </summary>
        private long _UnitPricePercent = 50;
        #endregion


        #region プロパティ
        /// <summary>
        /// 製品一覧
        /// </summary>
        public ListCollectionView ProductsView { get; }


        /// <summary>
        /// 単価(百分率)
        /// </summary>
        public double UnitPricePercent
        {
            get => _UnitPricePercent;
            set
            {
                _UnitPricePercent = (long)value;

                foreach (var product in _Model.Products)
                {
                    product.SetUnitPricePercent(_UnitPricePercent);
                }
                
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 選択されたアイテムを展開する
        /// </summary>
        public ICommand SelectedExpand { get; }

        /// <summary>
        /// 選択されたアイテムを折りたたむ
        /// </summary>
        public ICommand SelectedCollapse { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="productsGridModel">製品一覧用Model</param>
        public ProductsGridViewModel(ProductsGridModel productsGridModel)
        {
            _Model = productsGridModel;
            ProductsView = (ListCollectionView)CollectionViewSource.GetDefaultView(_Model.Products);

            // ソート方向設定
            ProductsView.SortDescriptions.Clear();
            ProductsView.SortDescriptions.Add(new SortDescription("Ware.WareGroup.Tier", ListSortDirection.Ascending));
            ProductsView.SortDescriptions.Add(new SortDescription("Ware.Name", ListSortDirection.Ascending));

            SelectedExpand = new DelegateCommand<DataGrid>(SelectedExpandCommand);
            SelectedCollapse = new DelegateCommand<DataGrid>(SelectedCollapseCommand);
        }

        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _Model.Dispose();
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
