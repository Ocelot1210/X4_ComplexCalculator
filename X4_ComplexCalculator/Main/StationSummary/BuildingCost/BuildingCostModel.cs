using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.ResourcesGrid;

namespace X4_ComplexCalculator.Main.StationSummary.BuildingCost
{
    /// <summary>
    /// 建造コスト用
    /// </summary>
    class BuildingCostModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 建造に必要なウェア一覧
        /// </summary>
        private readonly ObservablePropertyChangedCollection<ResourcesGridItem> Resources;

        /// <summary>
        /// 建造コスト
        /// </summary>
        private long _BuildingCost = 0;
        #endregion


        #region プロパティ
        /// <summary>
        /// 建造コスト詳細
        /// </summary>
        public ObservableRangeCollection<BuildingCostDetailsItem> BuildingCostDetails = new ObservableRangeCollection<BuildingCostDetailsItem>();

        /// <summary>
        /// 建造コスト
        /// </summary>
        public long BuildingCost
        {
            get
            {
                return _BuildingCost;
            }
            set
            {
                if (value != _BuildingCost)
                {
                    _BuildingCost = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resources"></param>
        public BuildingCostModel(ObservablePropertyChangedCollection<ResourcesGridItem> resources)
        {
            Resources = resources;
            Resources.CollectionChangedAsync += Resources_OnCollectionChangedAsync;
            Resources.CollectionPropertyChangedAsync += Resources_OnPropertyChangedAsync;
        }

        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            Resources.CollectionChangedAsync -= Resources_OnCollectionChangedAsync;
            Resources.CollectionPropertyChangedAsync -= Resources_OnPropertyChangedAsync;
            BuildingCostDetails.Clear();
        }

        /// <summary>
        /// 建造に必要なウェア一覧のプロパティに変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task Resources_OnPropertyChangedAsync(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is ResourcesGridItem resource))
            {
                return;
            }

            switch (e.PropertyName)
            {
                // 個数変更時
                case nameof(ResourcesGridItem.Amount):
                    {
                        var item = BuildingCostDetails.Where(x => x.WareID == resource.Ware.WareID).First();
                        BuildingCost = BuildingCost - item.TotalPrice + resource.Price;
                        item.Count = resource.Amount;
                    }
                    break;

                // 単価変更時
                case nameof(ResourcesGridItem.UnitPrice):
                    {
                        var item = BuildingCostDetails.Where(x => x.WareID == resource.Ware.WareID).First();
                        BuildingCost = BuildingCost - item.TotalPrice + resource.Price;
                        item.UnitPrice = resource.UnitPrice;
                    }
                    break;

                default:
                    break;
            }

            await Task.CompletedTask;
        }


        /// <summary>
        /// 建造に必要なウェア一覧に変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task Resources_OnCollectionChangedAsync(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                var addItems = e.NewItems.Cast<ResourcesGridItem>()
                                         .Select(x => new BuildingCostDetailsItem(x.Ware.WareID, x.Ware.Name, x.Amount, x.UnitPrice));

                BuildingCostDetails.AddRange(addItems);
            }

            if (e.OldItems != null)
            {
                var removeItems = e.OldItems.Cast<ResourcesGridItem>().ToArray();
                BuildingCostDetails.RemoveAll(x => removeItems.Where(y => x.WareID == y.Ware.WareID).Any());
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                BuildingCostDetails.Clear();
            }


            BuildingCost = BuildingCostDetails.Sum(x => x.TotalPrice);
            
            await Task.CompletedTask;
        }
    }
}
