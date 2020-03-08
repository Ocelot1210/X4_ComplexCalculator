using System.Collections.ObjectModel;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.ResourcesGrid
{
    /// <summary>
    /// 建造に必要なリソースを表示するDataGridView用ViewModel
    /// </summary>
    class ResourcesGridViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 建造に必要なリソースを表示するDataGridView用Model
        /// </summary>
        readonly ResourcesGridModel Model;

        /// <summary>
        /// 価格割合
        /// </summary>
        long _UnitPricePercent = 50;
        #endregion

        #region プロパティ
        /// <summary>
        /// 建造に必要なリソース一覧
        /// </summary>
        public ObservableCollection<ResourcesGridItem> BuildResource => Model.Resources;


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

                foreach (var resource in BuildResource)
                {
                    resource.SetUnitPricePercent(_UnitPricePercent);
                }

                OnPropertyChanged();
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">建造リソース用Model</param>
        public ResourcesGridViewModel(ResourcesGridModel resourcesGridModel)
        {
            Model = resourcesGridModel;
        }
    }
}
