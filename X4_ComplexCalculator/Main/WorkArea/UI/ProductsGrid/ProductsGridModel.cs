using Prism.Mvvm;
using System;
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
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// 製品一覧用DataGridViewのModel
    /// </summary>
    class ProductsGridModel : BindableBase, IDisposable
    {
        #region メンバ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        readonly ObservablePropertyChangedCollection<ModulesGridItem> _Modules;

        /// <summary>
        /// 生産性
        /// </summary>
        private double _Efficiency;
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

            modules.CollectionChangedAsync += OnModulesChanged;
            modules.CollectionPropertyChangedAsync += OnModulePropertyChanged;

            _Modules = modules;
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            Products.Clear();
            _Modules.CollectionChangedAsync -= OnModulesChanged;
            _Modules.CollectionPropertyChangedAsync -= OnModulePropertyChanged;
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
            if (0 < module.Module.MaxWorkers ||
                0 < module.Module.WorkersCapacity ||
                module.Module.ModuleType.ModuleTypeID == "production" ||
                module.Module.ModuleType.ModuleTypeID == "habitation"
                )
            {
                if (!(e is PropertyChangedExtendedEventArgs<long> ev))
                {
                    return;
                }

                OnModuleCountChanged(module, ev.OldValue);
                UpdateEfficiency();
            }

            await Task.CompletedTask;
        }


        /// <summary>
        /// 生産性を計算
        /// </summary>
        /// <returns>生産性</returns>
        private double CalcEfficiency()
        {
            var ret = 0.0;

            var maxWorkers = _Modules.Sum(x => x.Module.MaxWorkers * x.ModuleCount);
            var workersCapacity = _Modules.Sum(x => x.Module.WorkersCapacity * x.ModuleCount);

            // 生産性を0.0以上、1.0以下にする
            if (0 < workersCapacity)
            {
                ret = (maxWorkers < workersCapacity) ? 1.0 : (double)workersCapacity / maxWorkers;
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
            if (e.NewItems != null)
            {
                OnModuleAdded(e.NewItems.Cast<ModulesGridItem>());
            }

            if (e.OldItems != null)
            {
                OnModuleRemoved(e.OldItems.Cast<ModulesGridItem>());
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Products.Clear();
            }

            UpdateEfficiency();

            await Task.CompletedTask;
        }


        /// <summary>
        /// 生産性を更新
        /// </summary>
        private void UpdateEfficiency()
        {
            // 生産性(倍率)
            double efficiency = CalcEfficiency();

            // 生産性が変化しない場合、何もしない
            if (efficiency == _Efficiency)
            {
                return;
            }

            foreach (var prod in Products)
            {
                prod.SetEfficiency(efficiency);
            }

            _Efficiency = efficiency;
        }


        /// <summary>
        /// モジュール数変更時
        /// </summary>
        /// <param name="module">変更があったモジュール</param>
        /// <param name="prevModuleCount">変更前モジュール数</param>
        private void OnModuleCountChanged(ModulesGridItem module, long prevModuleCount)
        {
            ModulesGridItem[] modules = { module };

            Dictionary<string, List<ProductDetailsListItem>> prodDict = AggregateProduct(modules);

            foreach (var item in prodDict)
            {
                // 変更対象のウェアを検索
                Products.Where(x => x.Ware.WareID == item.Key).FirstOrDefault()?.SetDetails(item.Value, prevModuleCount);
            }
        }


        /// <summary>
        /// モジュールが追加された場合
        /// </summary>
        /// <param name="addedModules">追加されたモジュール</param>
        private void OnModuleAdded(IEnumerable<ModulesGridItem> addedModules)
        {
            // 生産品集計用ディクショナリ
            Dictionary<string, List<ProductDetailsListItem>> prodDict = AggregateProduct(addedModules);

            var addItems = new List<ProductsGridItem>();
            foreach (var item in prodDict)
            {
                // すでにウェアが存在するか検索
                var prod = Products.Where(x => x.Ware.WareID == item.Key).FirstOrDefault();
                if (prod != null)
                {
                    // ウェアが一覧にある場合
                    prod.AddDetails(item.Value);
                }
                else
                {
                    // ウェアが一覧に無い場合
                    addItems.Add(new ProductsGridItem(item.Key, item.Value));
                }
            }

            Products.AddRange(addItems);
        }


        /// <summary>
        /// モジュールが削除された場合
        /// </summary>
        /// <param name="removedModules">追加されたモジュール</param>
        private void OnModuleRemoved(IEnumerable<ModulesGridItem> removedModules)
        {
            // 生産品集計用ディクショナリ
            Dictionary<string, List<ProductDetailsListItem>> prodDict = AggregateProduct(removedModules);

            foreach (var item in prodDict)
            {
                // 一致するウェアの詳細情報を削除
                Products.Where(x => x.Ware.WareID == item.Key).FirstOrDefault()?.RemoveDetails(item.Value);
            }

            Products.RemoveAll(x => !x.Details.Any());
        }


        /// <summary>
        /// 製品情報を集計
        /// </summary>
        /// <param name="targetModules">集計対象モジュール</param>
        /// <returns>集計結果</returns>
        private Dictionary<string, List<ProductDetailsListItem>> AggregateProduct(IEnumerable<ModulesGridItem> targetModules)
        {
            // 生産品集計用ディクショナリ
            var prodDict = new Dictionary<string, List<ProductDetailsListItem>>();    // <ウェアID, 詳細情報>

            // 処理対象モジュール一覧
            var modules = targetModules.Where(x => 0 < x.Module.MaxWorkers || 
                                                   0 < x.Module.WorkersCapacity ||
                                                   x.Module.ModuleType.ModuleTypeID == "production" ||
                                                   x.Module.ModuleType.ModuleTypeID == "habitation")
                          .GroupBy(x => x.Module.ModuleID)
                          .Select(x =>
                          {
                              var module = x.First().Module;
                              return (module.ModuleID, module.ModuleType.ModuleTypeID, Count: x.Sum(y => y.ModuleCount));
                          });

            // 処理対象モジュールが無ければ何もしない
            if (!modules.Any())
            {
                return prodDict;
            }

            // パラメータ設定
            var prodParam = new SQLiteCommandParameters(2);
            var habParam = new SQLiteCommandParameters(2);

            foreach (var module in modules)
            {
                switch (module.ModuleTypeID)
                {
                    // 製造モジュールの場合
                    case "production":
                        prodParam.Add("count", DbType.Int32, module.Count);
                        prodParam.Add("moduleID", DbType.String, module.ModuleID);
                        break;

                    // 居住モジュールの場合
                    case "habitation":
                        habParam.Add("count", DbType.Int32, module.Count);
                        habParam.Add("moduleID", DbType.String, module.ModuleID);
                        break;

                    default:
                        break;
                }
            }

            // 製造モジュール集計
            AggregateProductionModule(prodParam, prodDict);

            // 居住モジュール集計
            AggregateHabitationModule(habParam, prodDict);

            return prodDict;
        }


        /// <summary>
        /// 製造モジュールを更新
        /// </summary>
        /// <param name="param">製造モジュール情報のパラメータ</param>
        /// <param name="moduleDict">生産情報辞書</param>
        private void AggregateProductionModule(SQLiteCommandParameters param, Dictionary<string, List<ProductDetailsListItem>> prodDict)
        {
            //------------//
            // 製品を集計 //
            //------------//
            var query = $@"
SELECT
    WareProduction.WareID,
    WareEffect.Product AS Efficiency,
	CAST(Amount * (3600 / WareProduction.Time) AS INTEGER) AS Amount,
    :count AS Count,
    ModuleProduct.ModuleID
    
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
	NeedWareID  AS 'WareID',
    -1.0        AS Efficiency,
	CAST(-3600 / WareProduction.Time * WareResource.Amount AS INTEGER) AS Amount,
    :count      AS Count,
    :moduleID   AS ModuleID
FROM
	WareProduction,
	WareResource,
	ModuleProduct

WHERE
	ModuleProduct.ModuleID  = :moduleID AND
	ModuleProduct.WareID    = WareResource.WareID AND
	WareProduction.Method   = WareResource.Method AND
	ModuleProduct.WareID    = WareProduction.WareID AND
	WareResource.Method     = (
		SELECT
			DISTINCT ModuleProduct.Method
		FROM
			ModuleProduct,
			WareResource	
		WHERE
			ModuleProduct.ModuleID  = :moduleID AND
			ModuleProduct.Method    = WareResource.Method AND
			ModuleProduct.WareID    = WareResource.WareID
		UNION ALL
			SELECT 'default'
		LIMIT 1
	)";

            DBConnection.X4DB.ExecQuery(query, param, SumProduct, prodDict);
        }



        /// <summary>
        /// 居住モジュール情報集計処理
        /// </summary>
        /// <param name="param">居住モジュール情報のパラメータ</param>
        /// <param name="wareDict">ウェア情報辞書</param>
        /// <param name="moduleDict">関連モジュール情報辞書</param>
        private void AggregateHabitationModule(SQLiteCommandParameters param, Dictionary<string, List<ProductDetailsListItem>> prodDict)
        {
            var query = $@"
SELECT
	WorkUnitResource.WareID,
	CAST((CAST(WorkUnitResource.Amount AS REAL)	/ WorkUnitProduction.Amount) * -Module.WorkersCapacity AS INTEGER) As Amount,
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
            DBConnection.X4DB.ExecQuery(query, param, SumResource, prodDict);
        }


        /// <summary>
        /// 製品を集計
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// args[0] = Dictionary<string, List<ProductDetailsListItem>> : 製品集計用ディクショナリ
        /// </remarks>
        private void SumProduct(SQLiteDataReader dr, object[] args)
        {
            var wareID = (string)dr["WareID"];
            var amount = (long)dr["Amount"];

            // 関連モジュール集計
            var prodDict = (Dictionary<string, List<ProductDetailsListItem>>)args[0];
            var moduleID = (string)dr["ModuleID"];
            var moduleCount = (long)dr["Count"];
            var efficiency = (double)dr["Efficiency"];

            // このウェアに対して初回か？
            if (!prodDict.ContainsKey(wareID))
            {
                prodDict.Add(wareID, new List<ProductDetailsListItem>());
            }

            // このウェアに対し、既にモジュールが追加されているか？
            var itm = prodDict[wareID].Where(x => x.ModuleID == moduleID).FirstOrDefault();
            if (itm != null)
            {
                // すでにモジュールが追加されている場合、モジュール数を増やしてレコードがなるべく少なくなるようにする
                itm.Incriment(moduleCount);
            }
            else
            {
                // 新規追加
                prodDict[wareID].Add(new ProductDetailsListItem(moduleID, moduleCount, efficiency, amount));
            }
        }


        /// <summary>
        /// 生産に必要なウェアを集計
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// args[0] = Dictionary<string, List<ProductDetailsListItem>> : 製品集計用ディクショナリ
        /// </remarks>
        private void SumResource(SQLiteDataReader dr, object[] args)
        {
            var wareID = (string)dr["WareID"];
            var amount = (long)dr["Amount"];

            // 関連モジュール集計
            var prodDict = (Dictionary<string, List<ProductDetailsListItem>>)args[0];
            var moduleID = (string)dr["ModuleID"];
            var moduleCount = (long)dr["Count"];

            // このウェアに対して関連モジュール追加が初回か？
            if (!prodDict.ContainsKey(wareID))
            {
                prodDict.Add(wareID, new List<ProductDetailsListItem>());
            }

            // このウェアに対し、既にモジュールが追加されているか？
            var itm = prodDict[wareID].Where(x => x.ModuleID == moduleID).FirstOrDefault();
            if (itm != null)
            {
                // すでにモジュールが追加されている場合、モジュール数を増やしてレコードがなるべく少なくなるようにする
                itm.Incriment(moduleCount);
            }
            else
            {
                // 新規追加
                prodDict[wareID].Add(new ProductDetailsListItem(moduleID, moduleCount, -1, amount));
            }
        }
    }
}
