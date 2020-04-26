using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.ModulesGrid;

namespace X4_ComplexCalculator.Main.ProductsGrid
{
    /// <summary>
    /// 製品一覧用DataGridViewのModel
    /// </summary>
    class ProductsGridModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        readonly IReadOnlyCollection<ModulesGridItem> Modules;
        #endregion

        #region プロパティ
        /// <summary>
        /// 製品一覧
        /// </summary>
        public ObservablePropertyChangedCollection<ProductsGridItem> Products { get; private set; }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧</param>
        public ProductsGridModel(ObservablePropertyChangedCollection<ModulesGridItem> modules)
        {
            Products = new ObservablePropertyChangedCollection<ProductsGridItem>();

            modules.OnCollectionChangedAsync += OnModulesChanged;
            modules.OnCollectionPropertyChangedAsync += OnModulePropertyChanged;

            Modules = modules;
        }


        /// <summary>
        /// モジュールのプロパティが変更された場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnModulePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // モジュール数変更時以外は処理しない
            if (e.PropertyName != "ModuleCount")
            {
                await Task.CompletedTask;
                return;
            }

            if(!(sender is ModulesGridItem module))
            {
                await Task.CompletedTask;
                return;
            }

            // 製造モジュールか居住モジュールの場合のみ更新
            if (module.Module.ModuleType.ModuleTypeID == "production" ||
                module.Module.ModuleType.ModuleTypeID == "habitation"
                )
            {
                UpdateProducts();
            }

            await Task.CompletedTask;
        }


        /// <summary>
        /// 処理対象モジュール一覧取得
        /// </summary>
        /// <returns>処理対象モジュール一覧</returns>
        /// <remarks>
        /// 製造モジュールか居住モジュールのみ抽出し、モジュールIDごとにモジュール数を集計
        /// </remarks>
        private ProductionModuleInfo[] GetProductionModules()
        {
            return Modules.Where(x => x.Module.ModuleType.ModuleTypeID == "production" || x.Module.ModuleType.ModuleTypeID == "habitation")
                          .GroupBy(x => x.Module.ModuleID)
                          .Select(x =>
                          {
                              var module = x.First().Module;
                              return new ProductionModuleInfo(module.ModuleID, module.MaxWorkers, module.WorkersCapacity, module.ModuleType.ModuleTypeID, x.Sum(y => y.ModuleCount));
                          })
                          .ToArray();
        }

        /// <summary>
        /// 生産性を計算
        /// </summary>
        /// <returns>生産性</returns>
        private double CalcEfficiency(ProductionModuleInfo[] modules)
        {
            var ret = 0.0;

            var maxWorkers = modules.Sum(x => x.MaxWorkers * x.Count);
            var workersCapacity = modules.Sum(x => x.WorkersCapacity * x.Count);

            // 生産性を0.0以上、1.0以下にする
            if (0 < workersCapacity)
            {
                ret = (maxWorkers < workersCapacity) ? 1.0 : (double)maxWorkers / workersCapacity;
            }

            return ret;
        }

        /// <summary>
        /// 製品更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task OnModulesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateProducts();
            await Task.CompletedTask;
        }


        /// <summary>
        /// 製品を更新
        /// </summary>
        private void UpdateProducts()
        {
            // 処理対象モジュール一覧
            ProductionModuleInfo[] modules = GetProductionModules();

            // 処理対象が無ければクリアして終わる
            if (!modules.Any())
            {
                Products.Clear();
                return;
            }

            // 生産性(倍率)
            double efficiency = CalcEfficiency(modules);


            // ウェア集計用ディクショナリ
            var wareDict = new Dictionary<string, long>();      // <ウェアID, 生産数>

            // ウェア関連モジュール集計用ディクショナリ
            var moduleDict = new Dictionary<string, List<ProductDetailsListItem>>();    // <ウェアID, 詳細情報>

            // パラメータ設定
            var prodParam = new SQLiteCommandParameters(3);     // 製造モジュール用パラメーター
            var habParam = new SQLiteCommandParameters(2);      // 居住モジュール用パラメーター
            foreach (var module in modules)
            {
                switch (module.ModuleTypeID)
                {
                    case "production":
                        prodParam.Add("efficiency", DbType.Double, efficiency);
                        prodParam.Add("count", DbType.Int32, module.Count);
                        prodParam.Add("moduleID", DbType.String, module.ModuleID);
                        break;

                    case "habitation":
                        habParam.Add("count", DbType.Int32, module.Count);
                        habParam.Add("moduleID", DbType.String, module.ModuleID);
                        break;

                    default:
                        break;
                }
            }

            // 集計
            Task.WaitAll(
                // 製造モジュール集計
                AggregateProductionModule(prodParam, wareDict, moduleDict),

                // 居住モジュール集計
                AggregateHabitationModule(habParam, wareDict, moduleDict)
            );

            // 前回値保存
            var backup = Products.ToDictionary(x => x.Ware.WareID, x => new { x.UnitPrice, x.IsExpanded });

            var items = wareDict.Select(x =>
                                {
                                    var details = moduleDict[x.Key].OrderBy(x => x.ModuleName);
                                    var bak = new { UnitPrice = (long)0, IsExpanded = false };
                                    return backup.TryGetValue(x.Key, out bak)
                                        ? new ProductsGridItem(x.Key, x.Value, details, bak.IsExpanded, bak.UnitPrice)
                                        : new ProductsGridItem(x.Key, x.Value, details);
                                });


            Products.Reset(items.OrderBy(x => x.Ware.WareGroup.Tier).ThenBy(x => x.Ware.Name));
        }


        /// <summary>
        /// 製造モジュールを更新
        /// </summary>
        /// <param name="param">製造モジュール情報のパラメータ</param>
        /// <param name="wareDict">ウェア情報辞書</param>
        /// <param name="moduleDict">関連モジュール情報辞書</param>
        private async Task AggregateProductionModule(SQLiteCommandParameters param, Dictionary<string, long> wareDict, Dictionary<string, List<ProductDetailsListItem>> moduleDict)
        {
            //------------//
            // 製品を集計 //
            //------------//
            var query = $@"
SELECT
    WareProduction.WareID,
    (1 + WareEffect.Product * :efficiency) AS Efficiency,
	CAST(Amount * :count * (3600 / WareProduction.Time) * (1 + WareEffect.Product * :efficiency) AS INTEGER) AS Amount,
    :count AS Count,
    :moduleID AS ModuleID
    
FROM
    WareProduction,
	WareEffect,
	ModuleProduct
WHERE
	WareProduction.WareID  = WareEffect.WareID AND
	WareProduction.WareID  = ModuleProduct.WareID AND 
	ModuleProduct.ModuleID = :moduleID AND
	WareProduction.Method  = WareEffect.Method AND
	WareProduction.Method  = 
		CASE
			WHEN ModuleProduct.Method IN (
				SELECT
					WareProduction.Method
				FROM
					ModuleProduct,
					WareProduction
				WHERE
					ModuleProduct.ModuleID = :moduleID AND
					ModuleProduct.WareID   = WareProduction.WareID AND
					ModuleProduct.Method   = WareProduction.Method
			) THEN ModuleProduct.Method
			ELSE 'default'
		END

-- 生産に必要なウェアを連結

UNION ALL
SELECT
	NeedWareID AS 'WareID',
    -1.0 AS Efficiency,
	:count * CAST(-3600 / Time * WareResource.Amount AS INTEGER) AS Amount,
    :count AS Count,
    :moduleID AS ModuleID
FROM
	WareProduction,
	WareResource,
	ModuleProduct
WHERE
	ModuleProduct.ModuleID  = :moduleID AND
	ModuleProduct.WareID    = WareResource.WareID AND
	WareResource.Method     = WareProduction.Method AND
	WareResource.NeedWareID = WareProduction.WareID
";

            DBConnection.X4DB.ExecQuery(query, param, SumProduct, wareDict, moduleDict);

            await Task.CompletedTask;
        }



        /// <summary>
        /// 居住モジュール情報集計処理
        /// </summary>
        /// <param name="param">居住モジュール情報のパラメータ</param>
        /// <param name="wareDict">ウェア情報辞書</param>
        /// <param name="moduleDict">関連モジュール情報辞書</param>
        private async Task AggregateHabitationModule(SQLiteCommandParameters param, Dictionary<string, long> wareDict, Dictionary<string, List<ProductDetailsListItem>> moduleDict)
        {
            var query = $@"
SELECT
	WorkUnitResource.WareID,
	CAST((CAST(WorkUnitResource.Amount AS REAL)	/ WorkUnitProduction.Amount) * -Module.WorkersCapacity AS INTEGER) * :count As Amount,
	:count AS Count,
    :moduleID AS ModuleID
FROM
	WorkUnitProduction,
	WorkUnitResource,
	Module
	
WHERE
	WorkUnitResource.WorkUnitID = WorkUnitProduction.WorkUnitID AND
	WorkUnitResource.WorkUnitID = 'workunit_busy' AND
    Module.ModuleID             = :moduleID AND
    WorkUnitResource.Method     = WorkUnitProduction.Method AND
    WorkUnitResource.Method     =
	(
		SELECT
			RaceID
		FROM
			(
			SELECT
				DISTINCT Race.RaceID
			FROM
				ModuleOwner,
				Faction,
				Race,
				WorkUnitResource
			WHERE
				ModuleOwner.FactionID = Faction.FactionID AND
				Faction.RaceID = Race.RaceID AND
				WorkUnitResource.Method = Race.RaceID AND
				ModuleOwner.ModuleID = :moduleID
			UNION ALL
			SELECT 'default'
			)
		LIMIT 1
	)
";
            DBConnection.X4DB.ExecQuery(query, param, SumResource, wareDict, moduleDict);

            await Task.CompletedTask;
        }


        /// <summary>
        /// 製品を集計
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// args[0] = Dictionary<string, int> : ウェア集計用ディクショナリ
        /// args[1] = Dictionary<string, List<ProductDetailsListItem>> : ウェア詳細集計用ディクショナリ
        /// args[2] = string : モジュールID
        /// args[3] = int : モジュール数
        /// </remarks>
        private void SumProduct(SQLiteDataReader dr, object[] args)
        {
            var wareDict = (Dictionary<string, long>)args[0];


            var wareID = dr["WareID"].ToString();
            var amount = (long)dr["Amount"];

            // ディクショナリ内にウェアが存在するか？
            if (wareDict.ContainsKey(wareID))
            {
                // すでにウェアが存在している場合
                wareDict[wareID] += amount;
            }
            else
            {
                // 新規追加
                wareDict[wareID] = amount;
            }

            // 関連モジュール集計
            var moduleDict = (Dictionary<string, List<ProductDetailsListItem>>)args[1];
            var moduleID = (string)dr["ModuleID"];
            var moduleCount = (long)dr["Count"];
            var efficiency = (double)dr["Efficiency"];

            // このウェアに対して初回か？
            if (!moduleDict.ContainsKey(wareID))
            {
                moduleDict.Add(wareID, new List<ProductDetailsListItem>());
            }

            // このウェアに対し、既にモジュールが追加されているか？
            var itm = moduleDict[wareID].Where(x => x.ModuleID == moduleID);
            if (itm.Any())
            {
                // すでにモジュールが追加されている場合、モジュール数を増やしてレコードがなるべく少なくなるようにする
                itm.First().Incriment(moduleCount);
            }
            else
            {
                // 新規追加
                moduleDict[wareID].Add(new ProductDetailsListItem(moduleID, moduleCount, efficiency, amount));
            }
        }


        /// <summary>
        /// 製品を集計
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// args[0] = Dictionary<string, int> : ウェア集計用ディクショナリ
        /// args[1] = Dictionary<string, List<ProductDetailsListItem>> : ウェア詳細集計用ディクショナリ
        /// args[2] = string : モジュールID
        /// args[3] = int : モジュール数
        /// </remarks>
        private void SumResource(SQLiteDataReader dr, object[] args)
        {
            var dict = (Dictionary<string, long>)args[0];

            var wareID = dr["WareID"].ToString();
            var amount = (long)dr["Amount"];

            // ディクショナリ内にウェアが存在するか？
            if (dict.ContainsKey(wareID))
            {
                // すでにウェアが存在している場合
                dict[wareID] += amount;
            }
            else
            {
                // 新規追加
                dict[wareID] = amount;
            }


            // 関連モジュール集計
            var modulesDict = (Dictionary<string, List<ProductDetailsListItem>>)args[1];
            var moduleID = dr["ModuleID"].ToString();
            var moduleCount = (long)dr["Count"];

            // このウェアに対して関連モジュール追加が初回か？
            if (!modulesDict.ContainsKey(wareID))
            {
                modulesDict.Add(wareID, new List<ProductDetailsListItem>());
            }


            // このウェアに対し、既にモジュールが追加されているか？
            var itm = modulesDict[wareID].Where(x => x.ModuleID == moduleID);
            if (itm.Any())
            {
                // すでにモジュールが追加されている場合、モジュール数を増やしてレコードがなるべく少なくなるようにする
                itm.First().Incriment(moduleCount);
            }
            else
            {
                // 新規追加
                modulesDict[wareID].Add(new ProductDetailsListItem(moduleID, moduleCount, -1, amount));
            }
        }
    }
}
