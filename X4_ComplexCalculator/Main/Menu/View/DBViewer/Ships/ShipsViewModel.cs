using Prism.Mvvm;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB.X4DB;


namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Ships
{
    /// <summary>
    /// 艦船情報用ViewModel
    /// </summary>
    class ShipsViewModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// ウェア一覧
        /// </summary>
        private readonly ObservableRangeCollection<ShipsGridItem> _Ships = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// 表示用データ
        /// </summary>
        public ListCollectionView ShipsView { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ShipsViewModel()
        {
            // サイズ単位のエンジン
            var engines = Equipment.GetAll<Engine>()
                .GroupBy(x => x.Size.SizeID)
                .ToDictionary(x => x.Key, x => new EngineManager(x));

            // サイズごとの最大容量のシールド
            var maxCapacityShields = Equipment.GetAll<Shield>()
                .GroupBy(x => x.Size.SizeID)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.Capacity).First());

            // サイズ単位のスラスター
            var thrusters = Equipment.GetAll<Thruster>()
                .GroupBy(x => x.Size.SizeID)
                .ToDictionary(x => x.Key, x => new ThrusterManager(x));


            _Ships.Reset(Ship.GetAll().Select(x => new ShipsGridItem(x, engines, maxCapacityShields, thrusters)));

            ShipsView = (ListCollectionView)CollectionViewSource.GetDefaultView(_Ships);
            ShipsView.SortDescriptions.Clear();
            ShipsView.SortDescriptions.Add(new SortDescription("Ship.Name", ListSortDirection.Ascending));
        }
    }
}
