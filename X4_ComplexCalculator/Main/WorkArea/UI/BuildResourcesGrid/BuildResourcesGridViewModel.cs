using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid
{
    /// <summary>
    /// 建造に必要なリソースを表示するDataGridView用ViewModel
    /// </summary>
    class BuildResourcesGridViewModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// 建造に必要なリソースを表示するDataGridView用Model
        /// </summary>
        private readonly BuildResourcesGridModel _Model;


        /// <summary>
        /// 価格割合
        /// </summary>
        private long _UnitPricePercent = 50;


        /// <summary>
        /// 建造に必要なウェアを購入しない
        /// </summary>
        private bool _NoBuy;
        #endregion


        #region プロパティ
        /// <summary>
        /// 建造に必要なリソース一覧
        /// </summary>
        public ObservableCollection<BuildResourcesGridItem> BuildResource => _Model.Resources;


        /// <summary>
        /// 建造に必要なリソース一覧
        /// </summary>
        public ICollectionView BuildResourceView { get; }


        /// <summary>
        /// 単価(百分率)
        /// </summary>
        public double UnitPricePercent
        {
            get => _UnitPricePercent;
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


        /// <summary>
        /// 建造に必要なウェアを購入しない
        /// </summary>
        public bool NoBuy
        {
            get => _NoBuy;
            set
            {
                if (SetProperty(ref _NoBuy, value))
                {
                    foreach (var ware in BuildResource)
                    {
                        ware.NoBuy = value;
                    }
                }
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">建造リソース用Model</param>
        public BuildResourcesGridViewModel(BuildResourcesGridModel resourcesGridModel)
        {
            _Model = resourcesGridModel;

            BuildResourceView = new CollectionViewSource { Source = _Model.Resources }.View;
            BuildResourceView.SortDescriptions.Add(new SortDescription("Ware.Name", ListSortDirection.Ascending));
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _Model.Dispose();
        }
    }
}
