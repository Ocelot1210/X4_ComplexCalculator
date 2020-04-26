using System.Collections.ObjectModel;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.ModulesGrid;
using X4_ComplexCalculator.Main.StationSummary.WorkForce;
using X4_ComplexCalculator.Main.StationSummary.Profit;
using X4_ComplexCalculator.Main.ProductsGrid;
using X4_ComplexCalculator.Main.ResourcesGrid;
using System.ComponentModel;
using X4_ComplexCalculator.Main.StationSummary.BuildingCost;
using System.Windows;

namespace X4_ComplexCalculator.Main.StationSummary
{
    class StationSummaryViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 労働力用Model
        /// </summary>
        private readonly WorkForceModel WorkForceModel;

        /// <summary>
        /// 利益用Model
        /// </summary>
        private readonly ProfitModel ProfitModel;

        /// <summary>
        /// 建造コスト用Model
        /// </summary>
        private readonly BuildingCostModel BuildingCostModel;
        #endregion


        #region 労働力関連プロパティ
        /// <summary>
        /// 現在の労働力
        /// </summary>
        public long WorkForce => WorkForceModel.WorkForce;

        /// <summary>
        /// 必要労働力
        /// </summary>
        public long NeedWorkforce => WorkForceModel.NeedWorkforce;

        /// <summary>
        /// 労働力情報詳細
        /// </summary>
        public ObservableCollection<WorkForceDetailsItem> WorkforceDetails => WorkForceModel.WorkForceDetails;
        #endregion


        #region 損益関連プロパティ
        /// <summary>
        /// 1時間あたりの損益
        /// </summary>
        public long Profit => ProfitModel.Profit;

        /// <summary>
        /// 損益詳細
        /// </summary>
        public ObservableCollection<ProfitDetailsItem> ProfitDetails => ProfitModel.ProfitDetails;
        #endregion


        #region 建造コスト関連プロパティ
        /// <summary>
        /// 建造費用
        /// </summary>
        public long BuildingCost => BuildingCostModel.BuildingCost;


        /// <summary>
        /// 建造コスト詳細
        /// </summary>
        public ObservableCollection<BuildingCostDetailsItem> BuildingCostDetails => BuildingCostModel.BuildingCostDetails;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        /// <param name="products">製品一覧</param>
        /// <param name="resources">建造に必要なリソース一覧</param>
        public StationSummaryViewModel(ObservablePropertyChangedCollection<ModulesGridItem> modules, ObservablePropertyChangedCollection<ProductsGridItem> products, ObservablePropertyChangedCollection<ResourcesGridItem> resources)
        {
            WorkForceModel = new WorkForceModel(modules);
            WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(WorkForceModel, "PropertyChanged", ModelPropertyChanged);

            ProfitModel = new ProfitModel(products);
            WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(ProfitModel, "PropertyChanged", ModelPropertyChanged);

            BuildingCostModel = new BuildingCostModel(resources);
            WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(BuildingCostModel, "PropertyChanged", ModelPropertyChanged);
        }

        /// <summary>
        /// Modelのプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }
    }
}
