using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// 製品計算用クラス
    /// </summary>
    class ProductCalculator
    {
        #region スタティックメンバ
        /// <summary>
        /// 製品計算用シングルトンインスタンス
        /// </summary>
        private static ProductCalculator? _SingletonInstance;
        #endregion


        #region メンバ
        /// <summary>
        /// key = ModuleID
        /// </summary>
        private readonly IReadOnlyDictionary<string, (string WareID, string Method)> _ModuleProduct;


        /// <summary>
        /// ウェア生産時の追加効果一覧
        /// key = WareID
        /// Tuple&lt;string Method, string EffectID, double Product&gt;
        /// </summary>
        private readonly IReadOnlyDictionary<string, Tuple<string, string, double>[]> _WareEffect;


        /// <summary>
        /// ウェア生産に必要な時間一覧
        /// key = WareID
        /// Tuple&lt;string Method, long Amount, double Time&gt;
        /// </summary>
        private readonly IReadOnlyDictionary<string, Tuple<string, long, double>[]> _WareProduction;


        /// <summary>
        /// ウェア生産に必要なウェア一覧
        /// key = WareID
        /// Tuple&lt;string Method, string NeedWareID, long Amount&gt;
        /// </summary>
        private readonly IReadOnlyDictionary<string, Tuple<string, string, long>[]> _WareResource;


        /// <summary>
        /// 居住モジュールの所有種族一覧
        /// key = ModuleID
        /// </summary>
        private readonly IReadOnlyDictionary<string, (string RaceID, long Capacity)> _HabitationModuleOwners;


        /// <summary>
        /// 従業員が必要とするウェア一覧
        /// key = Method
        /// </summary>
        private readonly IReadOnlyDictionary<string, (string WareID, double Amount)[]> _WorkUnitWares;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ProductCalculator()
        {
            // モジュールが生産するウェア一覧作成を作成
            {
                var dict = new Dictionary<string, (string, string)>();

                DBConnection.X4DB.ExecQuery("SELECT * FROM ModuleProduct", (dr, _) =>
                {
                    dict.Add((string)dr["ModuleID"], ((string)dr["WareID"], (string)dr["Method"]));
                });

                _ModuleProduct = dict;
            }

            // ウェア生産時の追加効果一覧を作成
            {
                var dict = new Dictionary<string, List<Tuple<string, string, double>>>();

                DBConnection.X4DB.ExecQuery("SELECT * FROM WareEffect", (dr, _) =>
                {
                    var wareID = (string)dr["WareID"];
                    if (!dict.ContainsKey(wareID))
                    {
                        dict.Add(wareID, new List<Tuple<string, string, double>>());
                    }

                    dict[wareID].Add(new Tuple<string, string, double>((string)dr["Method"], (string)dr["EffectID"], (double)dr["Product"]));
                });

                _WareEffect = dict.ToDictionary(x => x.Key, x => x.Value.ToArray());
            }


            // ウェア生産に必要な時間一覧を作成
            {
                var dict = new Dictionary<string, List<Tuple<string, long, double>>>();

                DBConnection.X4DB.ExecQuery("SELECT * FROM WareProduction", (dr, _) =>
                {
                    var wareID = (string)dr["WareID"];
                    if (!dict.ContainsKey(wareID))
                    {
                        dict.Add(wareID, new List<Tuple<string, long, double>>());
                    }

                    dict[wareID].Add(new Tuple<string, long, double>((string)dr["Method"], (long)dr["Amount"], (double)dr["Time"]));
                });

                _WareProduction = dict.ToDictionary(x => x.Key, x => x.Value.ToArray());
            }


            // ウェア生産に必要なウェア一覧を作成
            {
                var dict = new Dictionary<string, List<Tuple<string, string, long>>>();

                DBConnection.X4DB.ExecQuery("SELECT * FROM WareResource", (dr, _) =>
                {
                    var wareID = (string)dr["WareID"];
                    if (!dict.ContainsKey(wareID))
                    {
                        dict.Add(wareID, new List<Tuple<string, string, long>>());
                    }

                    dict[wareID].Add(new Tuple<string, string, long>((string)dr["Method"], (string)dr["NeedWareID"], (long)dr["Amount"]));
                });

                _WareResource = dict.ToDictionary(x => x.Key, x => x.Value.ToArray());
            }

            // 従業員が必要とするウェア一覧を作成
            {
                var dict = new Dictionary<string, List<(string, double)>>();

                var query = @"
SELECT
	WorkUnitProduction.Method,
	WorkUnitResource.WareID,
	CAST(WorkUnitResource.Amount AS REAL) / WorkUnitProduction.Amount AS Amount
FROM
	WorkUnitProduction,
	WorkUnitResource
WHERE
	WorkUnitProduction.WorkUnitID = WorkUnitResource.WorkUnitID AND
	WorkUnitProduction.WorkUnitID = 'workunit_busy' AND
	WorkUnitProduction.Method = WorkUnitResource.Method";

                DBConnection.X4DB.ExecQuery(query, (dr, _) =>
                {
                    var method = (string)dr["Method"];
                    if (!dict.ContainsKey(method))
                    {
                        dict.Add(method, new List<(string, double)>());
                    }

                    dict[method].Add(((string)dr["WareID"], (double)dr["Amount"]));
                });

                _WorkUnitWares = dict.ToDictionary(x => x.Key, x => x.Value.ToArray());
            }

            // 居住モジュールの所有種族一覧を作成
            {
                var query = @"
SELECT
	DISTINCT Module.ModuleID,
	Module.WorkersCapacity,
	Race.RaceID
	
FROM
	ModuleOwner,
	Faction,
	Race,
	Module
	
WHERE
	Module.ModuleID = ModuleOwner.ModuleID AND
	ModuleOwner.FactionID = Faction.FactionID AND
	Faction.RaceID = Race.RaceID AND
	Module.ModuleTypeID = 'habitation'";

                var dict = new Dictionary<string, (string, long)>();
                DBConnection.X4DB.ExecQuery(query, (dr, _) =>
                {
                    dict.Add((string)dr["ModuleID"], ((string)dr["RaceID"], (long)dr["WorkersCapacity"]));
                });

                _HabitationModuleOwners = dict;
            }
        }


        /// <summary>
        /// 製品計算用クラスのインスタンス
        /// </summary>
        public static ProductCalculator Instance
        {
            get
            {
                // 未作成なら作成する
                if (_SingletonInstance == null)
                {
                    _SingletonInstance = new ProductCalculator();
                }

                return _SingletonInstance;
            }
        }


        /// <summary>
        /// 製品と必要ウェアを計算
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="settings">ステーションの設定</param>
        /// <returns>製品と必要ウェア</returns>
        public IEnumerable<(string WareID, long Amount, Dictionary<string, double>? Efficiency)> CalcProduction(string moduleID)
        {
            // モジュールの生産品を取得
            var (prodWareID, prodMethod) = _ModuleProduct[moduleID];

            // ウェア生産に必要な時間(候補)
            var wareProdArr = _WareProduction[prodWareID];

            // ウェア生産に必要な時間
            var wareProd = wareProdArr.Where(x => x.Item1 == prodMethod).FirstOrDefault() ??
                           wareProdArr.Where(x => x.Item1 == "default").FirstOrDefault();

            {
                // ウェア生産時の追加効果一覧
                var effectArr = _WareEffect[prodWareID];

                // ウェア生産時の追加効果一覧をウェア生産方式別に抽出
                var effects = effectArr.Where(x => x.Item1 == prodMethod);
                if (!effects.Any())
                {
                    effects = effectArr.Where(x => x.Item1 == "default");
                }

                yield return (prodWareID, (long)Math.Floor(wareProd.Item2 * (3600 / wareProd.Item3)), effects.ToDictionary(x => x.Item2, x => x.Item3));
            }

            // ウェア生産に必要なウェア一覧(候補)
            if (_WareResource.TryGetValue(prodWareID, out Tuple<string, string, long>[]? wareResourceArr))
            {
                // ウェア生産に必要なウェア一覧
                var wareResources = Enumerable.Empty<Tuple<string, string, long>>();

                if (prodMethod != "default")
                {
                    wareResources = wareResourceArr.Where(x => x.Item1 != "default" && x.Item1 == prodMethod);
                }

                if (!wareResources.Any())
                {
                    wareResources = wareResourceArr.Where(x => x.Item1 == "default");
                }

                foreach (var res in wareResources)
                {
                    yield return (res.Item2, (long)Math.Floor(-3600 / wareProd.Item3 * res.Item3), null);
                }
            }
        }


        /// <summary>
        /// 労働者に必要なウェアを計算
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="count">モジュール数</param>
        /// <returns>労働者に必要なウェア</returns>
        public IEnumerable<(string WareID, long Amount)> CalcHabitation(string moduleID)
        {
            // 居住モジュールの種族を取得
            if (!_HabitationModuleOwners.TryGetValue(moduleID, out (string raceID, long Capacity) module))
            {
                module.raceID = "default";
            }

            // 居住モジュールの種族に対応するウェア一覧を取得
            if (!_WorkUnitWares.TryGetValue(module.raceID, out (string WareID, double Amount)[]? wares))
            {
                wares = _WorkUnitWares["default"];
            }

            foreach (var (wareID, amount) in wares)
            {
                yield return (wareID, (long)Math.Ceiling(-amount * module.Capacity));
            }
        }


        /// <summary>
        /// 必要モジュールを計算
        /// </summary>
        /// <param name="products">製品一覧</param>
        /// <param name="settings">ステーションの設定</param>
        public List<(string ModuleID, long Count)> CalcNeedModules(IReadOnlyList<ProductsGridItem> products, IStationSettings settings)
        {
            var addModules = new List<(string ModuleID, long Count)>();             // 追加予定モジュール
            var addModuleProducts = new List<(string WareID, long Count)>();        // 追加予定モジュールの製品一覧
            var excludeWares = new List<string>();                                  // 計算除外製品一覧

            foreach (var prod in products.Where(x => 0 < x.Ware.WareGroup.Tier).OrderBy(x => x.Ware.WareGroup.Tier))
            {
                // 追加予定のモジュールも含めた製造ウェア数
                var totalCount = prod.Count + addModuleProducts.Where(x => x.WareID == prod.Ware.WareID).FirstOrDefault().Count;

                // 不足していない or 計算除外ウェアの場合、何もしない
                if (0 <= totalCount || excludeWares.Contains(prod.Ware.WareID))
                {
                    continue;
                }

                // 不足しているウェアを製造するモジュールを検索
                var module = _ModuleProduct.Where(x => x.Value.WareID == prod.Ware.WareID && x.Value.Method == "default").FirstOrDefault();
                if (module.Key == null)
                {
                    module = _ModuleProduct.Where(x => x.Value.WareID == prod.Ware.WareID).FirstOrDefault();
                }

                // モジュールが製造するウェア数を計算
                var addProducts = CalcProduction(module.Key);
                var (_, addAmount, addEfficiency) = addProducts.First();
                if (addEfficiency?.ContainsKey("sunlight") ?? false)
                {
                    addAmount = (long)Math.Floor(addAmount * addEfficiency["sunlight"] * settings.Sunlight);
                }

                // 生産数が0の場合、計算除外製品一覧に突っ込む
                if (addAmount == 0)
                {
                    excludeWares.Add(prod.Ware.WareID);
                    continue;
                }

                // モジュールが製造するウェア数からあと何モジュール必要か計算する
                var modCount = (long)Math.Ceiling(-(double)totalCount / addAmount);


                // 追加予定モジュールにモジュールを追加
                addModules.Add((module.Key, modCount));

                // 追加予定モジュールの製品一覧を更新
                foreach (var (wareID, amount, _) in addProducts)
                {
                    addModuleProducts.Add((wareID, amount * modCount));
                }
            }

            return addModules;
        }
    }
}
