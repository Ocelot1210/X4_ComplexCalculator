using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// 製品計算用クラス
    /// </summary>
    class ProductCalclator
    {
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


        #region 静的メンバ
        /// <summary>
        /// 製品計算用インスタンス
        /// </summary>
        private static ProductCalclator? _SingletonProductCalclator;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private ProductCalclator()
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
        /// 製品計算用クラスのインスタンスを作成
        /// </summary>
        /// <returns>製品計算用クラスのインスタンス</returns>
        public static ProductCalclator Create()
        {
            // 未作成なら作成する
            if (_SingletonProductCalclator == null)
            {
                _SingletonProductCalclator = new ProductCalclator();
            }

            return _SingletonProductCalclator;
        }


        /// <summary>
        /// 製品と必要ウェアを計算
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="count">モジュール数</param>
        /// <returns>製品と必要ウェア</returns>
        public IEnumerable<(string WareID, long Amount, double Efficiency)> CalcProduction(string moduleID)
        {
            // モジュールの生産品を取得
            var modProd = _ModuleProduct[moduleID];

            // ウェア生産に必要な時間(候補)
            var wareProdArr = _WareProduction[modProd.WareID];

            // ウェア生産に必要な時間
            var wareProd = wareProdArr.Where(x => x.Item1 == modProd.Method).FirstOrDefault() ??
                           wareProdArr.Where(x => x.Item1 == "default").FirstOrDefault();

            {
                // モジュールの追加効果一覧(候補)
                var effectArr = _WareEffect[modProd.WareID];

                var effect = effectArr.Where(x => x.Item1 == modProd.Method).FirstOrDefault() ??
                             effectArr.Where(x => x.Item1 == "default").FirstOrDefault();

                yield return (modProd.WareID, (long)Math.Floor(wareProd.Item2 * (3600 / wareProd.Item3)) , effect.Item3);
            }

            // ウェア生産に必要なウェア一覧(候補)
            if (_WareResource.TryGetValue(modProd.WareID, out Tuple<string, string, long>[]? wareResourceArr))
            {
                // ウェア生産に必要なウェア一覧
                IEnumerable<Tuple<string, string, long>> wareResources = Enumerable.Empty<Tuple<string, string, long>>();

                if (modProd.Method != "default")
                {
                    wareResources = wareResourceArr.Where(x => x.Item1 != "default" && x.Item1 == modProd.Method);
                }

                if (wareResources == null || !wareResources.Any())
                {
                    wareResources = wareResourceArr.Where(x => x.Item1 == "default");
                }

                foreach (var res in wareResources)
                {
                    yield return (res.Item2, (long)Math.Floor(-3600 / wareProd.Item3 * res.Item3), -1.0);
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
            if (!_HabitationModuleOwners.TryGetValue(moduleID, out (string raceID, long Capacity) module))
            {
                module.raceID = "default";
            }

            if (!_WorkUnitWares.TryGetValue(module.raceID, out (string WareID, double Amount)[]? wares))
            {
                wares = _WorkUnitWares["default"];
            }

            foreach (var ware in wares)
            {
                yield return (ware.WareID, (long)Math.Ceiling(-ware.Amount * module.Capacity));
            }
        }


        /// <summary>
        /// 必要モジュールを計算
        /// </summary>
        /// <param name="products"></param>
        public List<(string ModuleID, long Count)> CalcNeedModules(IReadOnlyList<ProductsGridItem> products)
        {
            var addModules = new List<(string ModuleID, long Count)>();
            var addModuleProducts = new List<(string WareID, long Count)>();

            foreach (var prod in products.Where(x => 0 < x.Ware.WareGroup.Tier).OrderBy(x => x.Ware.WareGroup.Tier))
            {
                var totalCount = prod.Count + addModuleProducts.Where(x => x.WareID == prod.Ware.WareID).FirstOrDefault().Count;

                // 不足していなければ何もしない
                if (0 <= totalCount)
                {
                    continue;
                }

                var module = _ModuleProduct.Where(x => x.Value.WareID == prod.Ware.WareID && x.Value.Method == "default").FirstOrDefault();
                if (module.Key == null)
                {
                    module = _ModuleProduct.Where(x => x.Value.WareID == prod.Ware.WareID).FirstOrDefault();
                }

                var addProducts = CalcProduction(module.Key);
                var modCount = (long)Math.Ceiling(-(double)totalCount / addProducts.First().Amount);

                addModules.Add((module.Key, modCount));
                foreach (var p in addProducts)
                {
                    addModuleProducts.Add((p.WareID, p.Amount * modCount));
                }
            }

            return addModules;
        }
    }
}
