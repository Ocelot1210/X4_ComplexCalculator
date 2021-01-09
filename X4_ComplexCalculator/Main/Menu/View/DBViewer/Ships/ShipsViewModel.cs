using Prism.Mvvm;
using System.Collections.Generic;
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


            _Ships.Reset(Ship.GetAll().Select(x => new ShipsGridItem(x, GetEngineManagers(x, engines), GetShields(x, maxCapacityShields), GetThrusters(x, thrusters))));

            ShipsView = (ListCollectionView)CollectionViewSource.GetDefaultView(_Ships);
            ShipsView.SortDescriptions.Clear();
            ShipsView.SortDescriptions.Add(new SortDescription("Ship.Name", ListSortDirection.Ascending));
        }


        /// <summary>
        /// 艦船が使用すべき最良エンジン情報を取得する
        /// </summary>
        /// <param name="ship">対象の艦船</param>
        /// <param name="engines">サイズIDをキーにした最良エンジン情報</param>
        /// <returns>艦船が使用すべきサイズIDをキーにした最良エンジン情報</returns>
        private IReadOnlyDictionary<string, EngineManager> GetEngineManagers(Ship ship, IReadOnlyDictionary<string, EngineManager> engines)
        {
            // 艦船のデフォルトのエンジン情報を取得
            if (!ship.Loadouts.TryGetValue("default", out var defaultLoadouts))
            {
                return engines;
            }
            var shipEngines = defaultLoadouts.Select(x => x.Equipment).OfType<Engine>().ToArray();

            // デフォルトのエンジンが未設定なら最良のエンジンを返す
            if (!shipEngines.Any())
            {
                return engines;
            }

            return engines.Keys.ToDictionary(
                x => x,
                x =>
                {
                    var engine = shipEngines.FirstOrDefault(y => y.Size.SizeID == x);

                    return (engine is not null) ? new EngineManager(engine) : engines[x];
                }
                );
        }


        /// <summary>
        /// 艦船が使用すべきシールドを取得する
        /// </summary>
        /// <param name="ship">対象の艦船</param>
        /// <param name="shields">サイズIDをキーにした最良のシールド</param>
        /// <returns>艦船が使用すべきサイズIDをキーにした装備</returns>
        private IReadOnlyDictionary<string, Shield> GetShields(Ship ship, IReadOnlyDictionary<string, Shield> shields)
        {
            // 艦船のデフォルトのシールド情報を取得
            if (!ship.Loadouts.TryGetValue("default", out var defaultLoadouts))
            {
                return shields;
            }
            var shipEquipments = defaultLoadouts.Select(x => x.Equipment).OfType<Shield>().ToArray();

            // デフォルトの装備が未設定なら最良の装備を返す
            if (!shipEquipments.Any())
            {
                return shields;
            }

            return shields.Keys.ToDictionary(x => x, x => shipEquipments.FirstOrDefault(y => y.Size.SizeID == x) ?? shields[x]);
        }


        /// <summary>
        /// 艦船が使用すべきスラスターを取得する
        /// </summary>
        /// <param name="ship">艦船</param>
        /// <param name="thrusters">サイズIDをキーにした 最良のスラスター情報</param>
        /// <returns>艦船が使用すべきサイズIDをキーにした 最良のスラスター情報</returns>
        private IReadOnlyDictionary<string, ThrusterManager> GetThrusters(Ship ship, IReadOnlyDictionary<string, ThrusterManager> thrusters)
        {
            // 艦船のデフォルトのスラスター情報を取得
            if (!ship.Loadouts.TryGetValue("default", out var defaultLoadouts))
            {
                return thrusters;
            }

            // 艦船のデフォルトのスラスター情報を取得
            var shipThrusters = defaultLoadouts.Select(x => x.Equipment).OfType<Thruster>().ToArray();

            // デフォルトのスラスターが未設定なら最良の装備を返す
            if (!shipThrusters.Any())
            {
                return thrusters;
            }

            return thrusters.Keys.ToDictionary(
                x => x, 
                x =>
                {
                    var thruster = shipThrusters.FirstOrDefault(y => y.Size.SizeID == x);

                    return (thruster is not null) ? new ThrusterManager(thruster) : thrusters[x];
                }
                );
        }
    }
}
