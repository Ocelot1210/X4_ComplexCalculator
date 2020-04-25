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
        private readonly IReadOnlyCollection<ResourcesGridItem> Resources;

        /// <summary>
        /// 建造コスト
        /// </summary>
        private long _BuildingCost = 0;
        #endregion


        #region プロパティ
        /// <summary>
        /// 建造コスト詳細
        /// </summary>
        public SmartCollection<BuildingCostDetailsItem> BuildingCostDetails = new SmartCollection<BuildingCostDetailsItem>();

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
        public BuildingCostModel(MemberChangeDetectCollection<ResourcesGridItem> resources)
        {
            Resources = resources;
            resources.OnCollectionChangedAsync += Resources_OnCollectionChangedAsync;
            resources.OnCollectionPropertyChangedAsync += Resources_OnPropertyChangedAsync;
        }

        /// <summary>
        /// 建造に必要なウェア一覧のプロパティに変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task Resources_OnPropertyChangedAsync(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "UnitPrice")
            {
                return;
            }

            if (!(sender is ResourcesGridItem resource))
            {
                return;
            }

            var item = BuildingCostDetails.Where(x => x.WareID == resource.Ware.WareID).First();
            BuildingCost = BuildingCost - item.TotalPrice + resource.Price;
            item.UnitPrice = resource.UnitPrice;

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
            var buildingCost = 0L;

            var details = Resources.Select(x =>
            {
                var ret = new BuildingCostDetailsItem(x.Ware.WareID, x.Ware.Name, x.Count, x.UnitPrice);
                buildingCost += ret.TotalPrice;
                return ret;
            }).OrderBy(x => x.WareName);

            BuildingCostDetails.Reset(details);
            BuildingCost = buildingCost;
            
            await Task.CompletedTask;
        }
    }
}
