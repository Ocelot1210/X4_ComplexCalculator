using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.WorkArea.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.ResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.StationSummary.BuildingCost;
using X4_ComplexCalculator.Main.WorkArea.StationSummary.Profit;
using X4_ComplexCalculator.Main.WorkArea.StationSummary.WorkForce.ModuleInfo;
using X4_ComplexCalculator.Main.WorkArea.StationSummary.WorkForce.NeedWareInfo;

namespace X4_ComplexCalculator.Main.WorkArea.StationSummary
{
    class StationSummaryViewModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 労働力用Model
        /// </summary>
        private readonly WorkForceModuleInfoModel _WorkForceModuleInfoModel;

        /// <summary>
        /// 必要ウェア用Model
        /// </summary>
        private readonly NeedWareInfoModel _NeedWareInfoModel;

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
        public long WorkForce => _WorkForceModuleInfoModel.WorkForce;

        /// <summary>
        /// 必要労働力
        /// </summary>
        public long NeedWorkforce => _WorkForceModuleInfoModel.NeedWorkforce;

        /// <summary>
        /// 労働力関連モジュール情報
        /// </summary>
        public ObservableCollection<WorkForceModuleInfoDetailsItem> WorkforceModuleDetails => _WorkForceModuleInfoModel.WorkForceDetails;

        /// <summary>
        /// 必要ウェア情報
        /// </summary>
        public ListCollectionView WorkforceNeedWareCollectionView { get; }
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
            // 労働力関係初期化
            {
                _WorkForceModuleInfoModel = new WorkForceModuleInfoModel(modules);
                _WorkForceModuleInfoModel.PropertyChanged += WorkForceModuleInfo_PropertyChanged;
            }

            {
                _NeedWareInfoModel = new NeedWareInfoModel(products);

                WorkforceNeedWareCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(_NeedWareInfoModel.NeedWareInfoDetails);
                WorkforceNeedWareCollectionView.SortDescriptions.Clear();
                WorkforceNeedWareCollectionView.SortDescriptions.Add(new SortDescription(nameof(NeedWareInfoDetailsItem.Method), ListSortDirection.Ascending));
                WorkforceNeedWareCollectionView.SortDescriptions.Add(new SortDescription(nameof(NeedWareInfoDetailsItem.WareName), ListSortDirection.Ascending));
                WorkforceNeedWareCollectionView.GroupDescriptions.Clear();

                WorkforceNeedWareCollectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(NeedWareInfoDetailsItem.Method)));
            }


            // 損益関係初期化
            {
                _ProfitModel = new ProfitModel(products);
                _ProfitModel.PropertyChanged += ProfitModel_PropertyChanged;
            }


            // 建造コスト関係初期化
            {
                _BuildingCostModel = new BuildingCostModel(resources);
                _BuildingCostModel.PropertyChanged += BuildingCostModel_PropertyChanged;
            }
        }


        /// <summary>
        /// 労働力関連モジュール情報用Modelのプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkForceModuleInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WorkForceModuleInfoModel.NeedWorkforce):
                    RaisePropertyChanged(nameof(NeedWorkforce));
                    _NeedWareInfoModel.NeedWorkforce = _WorkForceModuleInfoModel.NeedWorkforce;
                    break;

                case nameof(WorkForceModuleInfoModel.WorkForce):
                    RaisePropertyChanged(nameof(WorkForce));
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 損益情報用Modelのプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProfitModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ProfitModel.Profit):
                    RaisePropertyChanged(nameof(Profit));
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// 建造コスト用Modelのプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BuildingCostModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BuildingCostModel.BuildingCost):
                    RaisePropertyChanged(nameof(BuildingCost));
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _WorkForceModuleInfoModel.PropertyChanged   -= WorkForceModuleInfo_PropertyChanged;
            _ProfitModel.PropertyChanged                -= ProfitModel_PropertyChanged;
            _BuildingCostModel.PropertyChanged          -= BuildingCostModel_PropertyChanged;

            _WorkForceModuleInfoModel.Dispose();
            _NeedWareInfoModel.Dispose();
            _ProfitModel.Dispose();
            _BuildingCostModel.Dispose();
        }
    }
}
