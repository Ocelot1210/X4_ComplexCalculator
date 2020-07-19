using Prism.Mvvm;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.BuildingCost
{
    /// <summary>
    /// 建造コスト用
    /// </summary>
    class BuildingCostModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 建造に必要なウェア一覧
        /// </summary>
        public ObservablePropertyChangedCollection<BuildResourcesGridItem> Resources;


        /// <summary>
        /// 建造コスト
        /// </summary>
        private long _BuildingCost = 0;
        #endregion


        #region プロパティ
        /// <summary>
        /// 建造コスト
        /// </summary>
        public long BuildingCost
        {
            get => _BuildingCost;
            set
            {
                if (value != _BuildingCost)
                {
                    _BuildingCost = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resources"></param>
        public BuildingCostModel(ObservablePropertyChangedCollection<BuildResourcesGridItem> resources)
        {
            Resources = resources;
            Resources.CollectionChanged += Resources_OnCollectionChanged;
            Resources.CollectionPropertyChanged += Resources_OnPropertyChanged;
        }

        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            Resources.CollectionChanged -= Resources_OnCollectionChanged;
            Resources.CollectionPropertyChanged -= Resources_OnPropertyChanged;
            Resources.Clear();
        }


        /// <summary>
        /// 建造に必要なウェア一覧のプロパティに変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void Resources_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is BuildResourcesGridItem))
            {
                return;
            }

            switch (e.PropertyName)
            {
                // 価格変更時
                case nameof(BuildResourcesGridItem.Price):
                    if (e is PropertyChangedExtendedEventArgs<long> ev)
                    {
                        BuildingCost -= (ev.OldValue - ev.NewValue);
                    }
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// 建造に必要なウェア一覧に変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void Resources_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                BuildingCost += e.NewItems.Cast<BuildResourcesGridItem>().Sum(x => x.Price);
            }

            if (e.OldItems != null)
            {
                BuildingCost -= e.OldItems.Cast<BuildResourcesGridItem>().Sum(x => x.Price);
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                BuildingCost = Resources.Sum(x => x.Price);
            }
        }
    }
}
