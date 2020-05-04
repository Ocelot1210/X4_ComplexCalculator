using System.Collections.ObjectModel;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.StationSummary.WorkForce;
using X4_ComplexCalculator.Main.WorkArea.StationSummary.Profit;
using X4_ComplexCalculator.Main.WorkArea.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.ResourcesGrid;
using System.ComponentModel;
using X4_ComplexCalculator.Main.WorkArea.StationSummary.BuildingCost;
using System.Windows;

namespace X4_ComplexCalculator.Main.WorkArea.StationSummary
{
    class StationSummaryViewModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 労働力用Model
        /// </summary>
        private readonly WorkForceModel _WorkForceModel;

        /// <summary>
        /// 利益用Model
        /// </summary>
        private readonly ProfitModel _ProfitModel;

        /// <summary>
        /// 建造コスト用Model
        /// </summary>
        private readonly BuildingCostModel _BuildingCostModel;
        #endregion


        #region 労働力関連プロパティ
        /// <summary>
        /// 現在の労働力
        /// </summary>
        public long WorkForce => _WorkForceModel.WorkForce;

        /// <summary>
        /// 必要労働力
        /// </summary>
        public long NeedWorkforce => _WorkForceModel.NeedWorkforce;

        /// <summary>
        /// 労働力情報詳細
        /// </summary>
        public ObservableCollection<WorkForceDetailsItem> WorkforceDetails => _WorkForceModel.WorkForceDetails;
        #endregion


        #region 損益関連プロパティ
        /// <summary>
        /// 1時間あたりの損益
        /// </summary>
        public long Profit => _ProfitModel.Profit;

        /// <summary>
        /// 損益詳細
        /// </summary>
        public ObservableCollection<ProfitDetailsItem> ProfitDetails => _ProfitModel.ProfitDetails;
        #endregion


        #region 建造コスト関連プロパティ
        /// <summary>
        /// 建造費用
        /// </summary>
        public long BuildingCost => _BuildingCostModel.BuildingCost;


        /// <summary>
        /// 建造コスト詳細
        /// </summary>
        public ObservableCollection<BuildingCostDetailsItem> BuildingCostDetails => _BuildingCostModel.BuildingCostDetails;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        /// <param name="products">製品一覧</param>
        /// <param name="resources">建造に必要なリソース一覧</param>
        public StationSummaryViewModel(ObservablePropertyChangedCollection<ModulesGridItem> modules, ObservablePropertyChangedCollection<ProductsGridItem> products, ObservablePropertyChangedCollection<ResourcesGridItem> resources)
        {
            _WorkForceModel = new WorkForceModel(modules);
            _WorkForceModel.PropertyChanged += ModelPropertyChanged;

            _ProfitModel = new ProfitModel(products);
            _ProfitModel.PropertyChanged += ModelPropertyChanged;

            _BuildingCostModel = new BuildingCostModel(resources);
            _BuildingCostModel.PropertyChanged += ModelPropertyChanged;
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


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _WorkForceModel.PropertyChanged -= ModelPropertyChanged;
            _ProfitModel.PropertyChanged -= ModelPropertyChanged;
            _BuildingCostModel.PropertyChanged -= ModelPropertyChanged;

            _WorkForceModel.Dispose();
            _ProfitModel.Dispose();
            _BuildingCostModel.Dispose();
        }
    }
}
