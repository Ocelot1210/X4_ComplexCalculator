using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea.ModulesGrid;
using System.Collections.Specialized;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common.Collection;
using System;

namespace X4_ComplexCalculator.Main.WorkArea.StoragesGrid
{
    /// <summary>
    /// 保管庫一覧表示用DataGridViewのModel
    /// </summary>
    class StoragesGridModel : IDisposable
    {
        #region メンバ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        readonly ObservablePropertyChangedCollection<ModulesGridItem> Modules;
        #endregion


        #region プロパティ
        /// <summary>
        /// ストレージ一覧
        /// </summary>
        public ObservableRangeCollection<StoragesGridItem> Storages { get; private set; } = new ObservableRangeCollection<StoragesGridItem>();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧</param>
        public StoragesGridModel(ObservablePropertyChangedCollection<ModulesGridItem> modules)
        {
            Modules = modules;
            Modules.CollectionChangedAsync += OnModulesChanged;
            Modules.CollectionPropertyChangedAsync += OnModulePropertyChanged;
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            Modules.CollectionChangedAsync -= OnModulesChanged;
            Modules.CollectionPropertyChangedAsync -= OnModulePropertyChanged;
        }

        /// <summary>
        /// モジュールのプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnModulePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // モジュール数変更時のみ処理
            if (e.PropertyName != "ModuleCount")
            {
                await Task.CompletedTask;
                return;
            }


            if (!(sender is ModulesGridItem module))
            {
                await Task.CompletedTask;
                return;
            }

            // 保管モジュールの場合のみ更新
            if (module.Module.ModuleType.ModuleTypeID == "storage")
            {
                OnModuleCountChanged(module);
            }

            await Task.CompletedTask;
        }


        /// <summary>
        /// モジュール一覧変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task OnModulesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                OnModulesAdded(e.NewItems.Cast<ModulesGridItem>());
            }

            if (e.OldItems != null)
            {
                OnModulesRemoved(e.OldItems.Cast<ModulesGridItem>());
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Storages.Clear();
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// モジュールが追加された時
        /// </summary>
        /// <param name="modules"></param>
        private void OnModulesAdded(IEnumerable<ModulesGridItem> modules)
        {
            Dictionary<string, List<StorageDetailsListItem>> storageModules = AggregateStorage(modules);

            var addTarget = new List<StoragesGridItem>();

            foreach (var kvp in storageModules)
            {
                // 一致するレコードを探す
                var itm = Storages.Where(x => x.TransportType.TransportTypeID == kvp.Key).FirstOrDefault();
                if (itm != null)
                {
                    // 既にレコードがある場合
                    itm.AddDetails(kvp.Value);
                }
                else
                {
                    // 初回追加の場合
                    addTarget.Add(new StoragesGridItem(kvp.Key, kvp.Value));
                }
            }

            Storages.AddRange(addTarget);
        }

        /// <summary>
        /// モジュールが削除された時
        /// </summary>
        /// <param name="modules"></param>
        private void OnModulesRemoved(IEnumerable<ModulesGridItem> modules)
        {
            Dictionary<string, List<StorageDetailsListItem>> storageModules = AggregateStorage(modules);

            foreach (var kvp in storageModules)
            {
                // 一致するレコードを探す
                var itm = Storages.Where(x => x.TransportType.TransportTypeID == kvp.Key).FirstOrDefault();
                if (itm != null)
                {
                    itm.RemoveDetails(kvp.Value);
                }
            }

            // 空のレコードを削除
            Storages.RemoveAll(x => x.Capacity == 0);
        }


        /// <summary>
        /// モジュール数変更時
        /// </summary>
        /// <param name="module"></param>
        private void OnModuleCountChanged(ModulesGridItem module)
        {
            ModulesGridItem[] modules = { module };

            Dictionary<string, List<StorageDetailsListItem>> storageModules = AggregateStorage(modules);

            foreach (var kvp in storageModules)
            {
                // 変更対象のウェアを検索
                Storages.Where(x => x.TransportType.TransportTypeID == kvp.Key).FirstOrDefault()?.SetDetails(kvp.Value);
            }
        }


        /// <summary>
        /// モジュール情報を集計
        /// </summary>
        /// <param name="modules">集計対象</param>
        /// <returns>集計結果</returns>
        private Dictionary<string, List<StorageDetailsListItem>> AggregateStorage(IEnumerable<ModulesGridItem> modules)
        {
            // 保管庫情報集計用
            var modulesDict = new Dictionary<string, List<StorageDetailsListItem>>();

            var targetModules = modules.Where(x => x.Module.ModuleType.ModuleTypeID == "storage")
                                       .GroupBy(x => x.Module.ModuleID)
                                       .Select(x => (x.First().Module.ModuleID, Count: x.Sum(y => y.ModuleCount)));

            if (!targetModules.Any())
            {
                return modulesDict;
            }

            var query = $@"
SELECT
    TransportTypeID,
    Amount * :count AS Amount,
    :count AS Count,
    ModuleID

FROM
    ModuleStorage

WHERE
    ModuleID = :moduleID";

            var sqlParam = new SQLiteCommandParameters(2);
            foreach (var module in targetModules)
            {
                sqlParam.Add("moduleID", System.Data.DbType.String, module.ModuleID);
                sqlParam.Add("count", System.Data.DbType.Int32, module.Count);
            }

            // 容量をタイプ別に集計
            DBConnection.X4DB.ExecQuery(query, sqlParam, SumStorage, modulesDict);

            return modulesDict;
        }

//        /// <summary>
//        /// 保管庫情報更新
//        /// </summary>
//        private void UpdateStorages()
//        {
//            var modules = Modules.Where(x => x.Module.ModuleType.ModuleTypeID == "storage")
//                                 .GroupBy(x => x.Module.ModuleID)
//                                 .Select(x => (x.First().Module.ModuleID, Count : x.Sum(y => y.ModuleCount)));

//            // 処理対象が無ければクリアして終わる
//            if (!modules.Any())
//            {
//                Storages.Clear();
//                return;
//            }

//            var sqlParam = new SQLiteCommandParameters(2);

//            var query = $@"
//SELECT
//    TransportTypeID,
//    Amount * :count AS Amount,
//    :count AS Count,
//    ModuleID

//FROM
//    ModuleStorage

//WHERE
//    ModuleID = :moduleID";

//            // 保管モジュールのみ列挙)
//            foreach (var module in modules)
//            {
//                sqlParam.Add("moduleID", System.Data.DbType.String, module.ModuleID);
//                sqlParam.Add("count", System.Data.DbType.Int32, module.Count);
//            }


//            // カーゴ種別ごとの容量
//            var transportDict = new Dictionary<string, long>();  // transportTypeID, Capacity

//            // 保管庫情報集計用
//            var modulesDict = new Dictionary<string, List<StorageDetailsListItem>>();

//            // 容量をタイプ別に集計
//            DBConnection.X4DB.ExecQuery(query, sqlParam, SumStorage, transportDict, modulesDict);

//            // 前回値保存
//            var backup = Storages.ToDictionary(x => x.TransportType.TransportTypeID, x => x.IsExpanded);

//            // コレクションに設定
//            Storages.Reset(transportDict.Select(x =>
//            {
//                var details = modulesDict[x.Key].OrderBy(x => x.ModuleName).ToArray();
//                return backup.TryGetValue(x.Key, out bool expanded)
//                    ? new StoragesGridItem(x.Key, x.Value, details, expanded)
//                    : new StoragesGridItem(x.Key, x.Value, details);
//            })
//                .OrderBy(x => x.TransportType.Name)
//            );
//        }

        /// <summary>
        /// 保管庫の容量を集計
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// args[0] = Dictionary<string, List<StorageDetailsListItem>> : 保管庫情報集計用ディクショナリ
        /// </remarks>
        private void SumStorage(SQLiteDataReader dr, object[] args)
        {
            var transportTypeID = (string)dr["TransportTypeID"];

            // 関連モジュール集計
            var modulesDict = (Dictionary<string, List<StorageDetailsListItem>>)args[0];

            // このカーゴ種別に対して関連モジュール追加が初回か？
            if (!modulesDict.ContainsKey(transportTypeID))
            {
                modulesDict.Add(transportTypeID, new List<StorageDetailsListItem>());
            }

            var moduleID = (string)dr["ModuleID"];
            var moduleCount = (long)dr["Count"];

            // このカーゴ種別に対し、既にモジュールが追加されているか？
            var itm = modulesDict[transportTypeID].Where(x => x.ModuleID == moduleID).FirstOrDefault();
            if (itm != null)
            {
                // 既にモジュールが追加されている場合、モジュール数を増やしてレコードがなるべく少なくなるようにする
                itm.Incriment(moduleCount);
            }
            else
            {
                // 新規追加
                modulesDict[transportTypeID].Add(new StorageDetailsListItem(moduleID, moduleCount));
            }
        }
    }
}
