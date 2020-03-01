using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.ModulesGrid;

namespace X4_ComplexCalculator.Main.ProductsGrid
{
    /// <summary>
    /// 製品一覧用DataGridViewのModel
    /// </summary>
    class ProductsGridModel : INotifyPropertyChangedBace
    {
        #region プロパティ
        /// <summary>
        /// 製品一覧
        /// </summary>
        public SmartCollection<ProductsGridItem> Products { get; private set; }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧のModel</param>
        public ProductsGridModel(ModulesGridModel moduleGridModel)
        {
            Products = new SmartCollection<ProductsGridItem>();

            moduleGridModel.OnModulesChanged += UpdateProducts;
        }


        /// <summary>
        /// 製品を更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateProducts(object sender, NotifyCollectionChangedEventArgs e)
        {
            // 前回値保存
            var backup = Products.ToDictionary(x => x.Ware.WareID, x => new { x.UnitPrice, x.IsExpanded });

            Products.Clear();

            // モジュール一覧(製造モジュールか居住モジュールのみ抽出)
            var modules = ((IEnumerable<ModulesGridItem>)sender).Where(x => x.Module.ModuleType.ModuleTypeID == "production" || x.Module.ModuleType.ModuleTypeID == "habitation");

            // 生産性(倍率)
            double efficiency = CalcEffeciency(modules);

            // ウェア集計用ディクショナリ
            var wareDict = new Dictionary<string, long>();      // <ウェアID, 生産数>

            // ウェア関連モジュール集計用ディクショナリ
            var moduleDict = new Dictionary<string, List<ProductDetailsListItem>>();    // <ウェアID, 詳細情報>


            foreach (var module in modules)
            {
                switch(module.Module.ModuleType.ModuleTypeID)
                {
                    // 製造モジュールの場合
                    case "production":
                        AggregateProductionModule(module, efficiency, wareDict, moduleDict);
                        break;

                    // 居住モジュールの場合
                    case "habitation":
                        AggregateHabitationModule(module, wareDict, moduleDict);
                        break;

                    // それ以外の場合(ありえない)
                    default:
                        System.Diagnostics.Debug.Assert(false, "モジュール種別が不正です。");
                        break;
                }
            }



            Products.AddRange(wareDict.Select((System.Func<KeyValuePair<string, long>, ProductsGridItem>)(x => 
            {
                var details = moduleDict[x.Key];
                var bak = new { UnitPrice = (long)0, IsExpanded = false};
                return backup.TryGetValue(x.Key, out bak)
                    ? new ProductsGridItem(x.Key, x.Value, details, bak.IsExpanded, bak.UnitPrice)
                    : new ProductsGridItem(x.Key, x.Value, details);
            })));
        }


        /// <summary>
        /// 製造モジュール用集計処理
        /// </summary>
        /// <param name="module">モジュール情報</param>
        /// <param name="efficiency">生産性</param>
        /// <param name="wareDict">ウェア情報辞書</param>
        /// <param name="moduleDict">ウェア情報辞書の関連モジュール辞書</param>
        private void AggregateProductionModule(ModulesGridItem module, double efficiency, Dictionary<string, long> wareDict, Dictionary<string, List<ProductDetailsListItem>> moduleDict)
        {
            //------------//
            // 製品を集計 //
            //------------//
            {
                var query = $@"
SELECT
    WareProduction.WareID,
    (1 + WareEffect.Product * {efficiency}) AS Efficiency,
	CAST(Amount * {module.ModuleCount} * (3600 / WareProduction.Time) * (1 + WareEffect.Product * {efficiency}) AS INTEGER) AS Amount

FROM
    WareProduction,
	WareEffect,
	ModuleProduct

WHERE
	WareProduction.WareID  = WareEffect.WareID AND
	WareProduction.WareID  = ModuleProduct.WareID AND 
	ModuleProduct.ModuleID = '{module.Module.ModuleID}' AND
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
					ModuleProduct.ModuleID = '{module.Module.ModuleID}' AND
					ModuleProduct.WareID   = WareProduction.WareID AND
					ModuleProduct.Method   = WareProduction.Method
			) THEN ModuleProduct.Method
			ELSE 'default'
		END";


                DBConnection.X4DB.ExecQuery(query, SumProduct, wareDict, moduleDict, module.Module.ModuleID, module.ModuleCount);
            }

            //--------------------------//
            // 生産に必要なウェアを集計 //
            //--------------------------//
            {
                var query = $@"
SELECT
	NeedWareID AS 'WareID',
	{module.ModuleCount} * CAST(-3600 / Time * WareResource.Amount AS INTEGER) AS 'Amount'
FROM
	WareProduction,
	WareResource,
	ModuleProduct
WHERE
	WareProduction.WareID  = WareResource.WareID   AND
	WareProduction.WareID  = ModuleProduct.WareID  AND
	WareResource.WareID    = WareProduction.WareID AND
	ModuleProduct.ModuleID = '{module.Module.ModuleID}' AND
    WareResource.Method    = WareProduction.Method";

                DBConnection.X4DB.ExecQuery(query, SumResource, wareDict, moduleDict, module.Module.ModuleID, module.ModuleCount);
            }
        }

        /// <summary>
        /// 居住モジュール用集計処理
        /// </summary>
        /// <param name="module">モジュール情報</param>
        /// <param name="wareDict">ウェア情報辞書</param>
        /// <param name="moduleDict">ウェア情報辞書の関連モジュール辞書</param>
        private void AggregateHabitationModule(ModulesGridItem module, Dictionary<string, long> wareDict, Dictionary<string, List<ProductDetailsListItem>> moduleDict)
        {
            var query = $@"
SELECT
	WorkUnitResource.WareID,
	CAST((CAST(WorkUnitResource.Amount AS REAL)	/ WorkUnitProduction.Amount) * -Module.WorkersCapacity AS INTEGER) * {module.ModuleCount} As Amount
	
FROM
	WorkUnitProduction,
	WorkUnitResource,
	Module
	
WHERE
	WorkUnitResource.WorkUnitID = WorkUnitProduction.WorkUnitID AND
	WorkUnitResource.WorkUnitID = 'workunit_busy' AND
    Module.ModuleID             = '{module.Module.ModuleID}' AND
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
				ModuleOwner.ModuleID = '{module.Module.ModuleID}'
			UNION ALL
			SELECT 'default'
			)
		LIMIT 1
	)
";
            DBConnection.X4DB.ExecQuery(query, SumResource, wareDict, moduleDict, module.Module.ModuleID, module.ModuleCount);
        }

        /// <summary>
        /// 生産性を計算
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        /// <returns>倍率</returns>
        private double CalcEffeciency(IEnumerable<ModulesGridItem> modules)
        {
            long workers = 0;
            long needWorkers = 0;

            foreach(var module in modules.Select(x => x.Module))
            {
                needWorkers += module.MaxWorkers;
                workers += module.WorkersCapacity;
            }

            if (needWorkers == 0)
            {
                return 0.0;
            }

            return (needWorkers < workers) ? 1.0 : (double)workers / needWorkers;
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
            var moduleID = (string)args[2];
            var moduleCount = (int)args[3];
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
            var moduleID = (string)args[2];
            var moduleCount = (int)args[3];

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
