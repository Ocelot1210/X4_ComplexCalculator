﻿using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
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
        /// ウェア生産に必要なウェア一覧
        /// key = WareID
        /// Tuple&lt;string Method, string NeedWareID, long Amount&gt;
        /// </summary>
        private readonly IReadOnlyDictionary<string, IReadOnlyList<(string, string, long)>> _WareResource;


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
                const string query = "SELECT ModuleID, WareID, Method FROM ModuleProduct";
                _ModuleProduct = X4Database.Instance.Query<(string ModuleID, string WareID, string Method)>(query)
                    .ToDictionary(x => x.ModuleID, x => (x.WareID, x.Method));
            }


            // ウェア生産に必要なウェア一覧を作成
            {
                var dict = new Dictionary<string, List<Tuple<string, string, long>>>();

                const string query = "SELECT WareID, Method, NeedWareID, Amount FROM WareResource";
                _WareResource = X4Database.Instance.Query<(string WareID, string Method, string NeedWareID, long Amount)>(query)
                    .GroupBy(x => x.WareID)
                    .ToDictionary(x => x.Key, x => x.Select(y => (y.Method, y.NeedWareID, y.Amount)).ToArray() as IReadOnlyList<(string, string, long)>);
            }

            // 従業員が必要とするウェア一覧を作成
            {
                var dict = new Dictionary<string, List<(string, double)>>();

                var query = @"
SELECT
	WareResource.Method,
	WareResource.WareID,
	(CAST(WareResource.Amount AS REAL) / WareProduction.Amount) / (WareProduction.Time / 3600.0) AS Amount
	
FROM
	WareProduction,
	WareResource
	
WHERE
	WareResource.WareID = 'workunit_busy' AND
	WareResource.WareID = WareProduction.WareID AND
	WareResource.Method = WareProduction.Method";

                _WorkUnitWares = X4Database.Instance.Query<(string Method, string WareID, double Amount)>(query)
                    .GroupBy(x => x.Method)
                    .ToDictionary(x => x.Key, x => x.Select(y => (y.WareID, y.Amount)).ToArray());
            }

            // 居住モジュールの所有種族一覧を作成
            {
                var query = @"
SELECT
	DISTINCT Module.ModuleID,
	Module.WorkersCapacity,
	Race.RaceID
	
FROM
	WareOwner,
	Faction,
	Race,
	Module
	
WHERE
	Module.ModuleID     = WareOwner.WareID AND
	WareOwner.FactionID = Faction.FactionID AND
	Faction.RaceID      = Race.RaceID AND
	Module.ModuleTypeID = 'habitation'";

                _HabitationModuleOwners = X4Database.Instance.Query<(string ModuleID, long WorkersCapacity, string RaceID)>(query)
                    .ToDictionary(x => x.ModuleID, x => (x.RaceID, x.WorkersCapacity));
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
                if (_SingletonInstance is null)
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
        /// <returns>製品と必要ウェア</returns>
        public IEnumerable<(string WareID, long Amount, IReadOnlyDictionary<string, WareEffect>? Efficiency)> CalcProduction(string moduleID)
        {
            // モジュールの生産品を取得
            var product = Ware.TryGet<Module>(moduleID)?.ModuleProduct ?? throw new ArgumentException();
            
            // ウェア生産情報を取得
            var wareProduction = WareProduction.Get(product.WareID, product.Method) ?? throw new ArgumentException();
            {
                // ウェア生産時の追加効果一覧をウェア生産方式別に抽出
                var effects = WareEffect.Get(product.WareID, product.Method);

                yield return (product.WareID, (long)Math.Floor(wareProduction.Amount * (3600 / wareProduction.Time)), effects);
            }



            // ウェア生産に必要なウェア一覧(候補)
            if (_WareResource.TryGetValue(product.WareID, out var wareResourceArr))
            {
                // ウェア生産に必要なウェア一覧
                var wareResources = Enumerable.Empty<(string, string, long)>();

                if (product.Method != "default")
                {
                    wareResources = wareResourceArr.Where(x => x.Item1 != "default" && x.Item1 == product.Method);
                }

                if (!wareResources.Any())
                {
                    wareResources = wareResourceArr.Where(x => x.Item1 == "default");
                }

                foreach (var res in wareResources)
                {
                    yield return (res.Item2, (long)Math.Floor(-3600 / wareProduction.Time * res.Item3), null);
                }
            }
        }


        /// <summary>
        /// 労働者に必要なウェアを計算
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
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
                var totalCount = prod.Count + addModuleProducts.FirstOrDefault(x => x.WareID == prod.Ware.ID).Count;

                // 不足していない or 計算除外ウェアの場合、何もしない
                if (0 <= totalCount || excludeWares.Contains(prod.Ware.ID))
                {
                    continue;
                }

                // 不足しているウェアを製造するモジュールを検索
                var module = _ModuleProduct.FirstOrDefault(x => x.Value.WareID == prod.Ware.ID && x.Value.Method == "default");
                if (module.Key is null)
                {
                    module = _ModuleProduct.FirstOrDefault(x => x.Value.WareID == prod.Ware.ID);
                }

                // モジュールが製造するウェア数を計算
                var addProducts = CalcProduction(module.Key);
                var (_, addAmount, addEfficiency) = addProducts.First();
                if (addEfficiency?.ContainsKey("sunlight") ?? false)
                {
                    addAmount = (long)Math.Floor(addAmount * addEfficiency["sunlight"].Product * settings.Sunlight);
                }

                // 生産数が0の場合、計算除外製品一覧に突っ込む
                if (addAmount == 0)
                {
                    excludeWares.Add(prod.Ware.ID);
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
