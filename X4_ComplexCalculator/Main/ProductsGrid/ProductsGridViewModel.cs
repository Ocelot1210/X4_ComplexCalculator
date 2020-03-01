using System.Collections.ObjectModel;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main.ModulesGrid;

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
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧用Model</param>
        public ProductsGridViewModel(ModulesGridModel moduleGridModel)
        {
            Model = new ProductsGridModel(moduleGridModel);
        }
    }
}
