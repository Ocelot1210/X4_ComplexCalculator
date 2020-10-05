﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Prism.Mvvm;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.BuildResources;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.BuildingCost
{
    /// <summary>
    /// 建造コスト用
    /// </summary>
    class BuildingCostModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 建造リソース情報
        /// </summary>
        private readonly IBuildResourcesInfo _BuildResources;


        /// <summary>
        /// 建造コスト
        /// </summary>
        private long _BuildingCost = 0;
        #endregion


        #region プロパティ
        /// <summary>
        /// 建造リソース一覧
        /// </summary>
        public ObservableCollection<BuildResourcesGridItem> BuildResources => _BuildResources.BuildResources;


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
        public BuildingCostModel(IBuildResourcesInfo resources)
        {
            _BuildResources = resources;
            _BuildResources.BuildResources.CollectionChanged += Resources_OnCollectionChanged;
            _BuildResources.BuildResources.CollectionPropertyChanged += Resources_OnPropertyChanged;
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _BuildResources.BuildResources.CollectionChanged -= Resources_OnCollectionChanged;
            _BuildResources.BuildResources.CollectionPropertyChanged -= Resources_OnPropertyChanged;
            BuildResources.Clear();
        }


        /// <summary>
        /// 建造に必要なウェア一覧のプロパティに変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                BuildingCost = BuildResources.Sum(x => x.Price);
            }
        }
    }
}
