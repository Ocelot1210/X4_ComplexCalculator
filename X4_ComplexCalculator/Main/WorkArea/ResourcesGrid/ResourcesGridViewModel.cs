using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace X4_ComplexCalculator.Main.WorkArea.ResourcesGrid
{
    /// <summary>
    /// 建造に必要なリソースを表示するDataGridView用ViewModel
    /// </summary>
    class ResourcesGridViewModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// 建造に必要なリソースを表示するDataGridView用Model
        /// </summary>
        readonly ResourcesGridModel _Model;

        /// <summary>
        /// 価格割合
        /// </summary>
        long _UnitPricePercent = 50;
        #endregion

        #region プロパティ
        /// <summary>
        /// 建造に必要なリソース一覧
        /// </summary>
        public ObservableCollection<ResourcesGridItem> BuildResource => _Model.Resources;


        /// <summary>
        /// 建造に必要なリソース一覧
        /// </summary>
        public ICollectionView BuildResourceView { get; }

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

                RaisePropertyChanged();
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">建造リソース用Model</param>
        public ResourcesGridViewModel(ResourcesGridModel resourcesGridModel)
        {
            _Model = resourcesGridModel;

            BuildResourceView = CollectionViewSource.GetDefaultView(_Model.Resources);
            BuildResourceView.SortDescriptions.Add(new SortDescription("Ware.Name", ListSortDirection.Ascending));
        }

        public void Dispose()
        {
            _Model.Dispose();
        }
    }
}
