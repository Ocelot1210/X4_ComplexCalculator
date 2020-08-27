using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid
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
        readonly ObservablePropertyChangedCollection<ModulesGridItem> _Modules;
        #endregion


        #region プロパティ
        /// <summary>
        /// ストレージ一覧
        /// </summary>
        public ObservablePropertyChangedCollection<StoragesGridItem> Storages { get; private set; } = new ObservablePropertyChangedCollection<StoragesGridItem>();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧</param>
        public StoragesGridModel(ObservablePropertyChangedCollection<ModulesGridItem> modules)
        {
            _Modules = modules;
            _Modules.CollectionChangedAsync += OnModulesChanged;
            _Modules.CollectionPropertyChangedAsync += OnModulePropertyChanged;
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            _Modules.CollectionChangedAsync -= OnModulesChanged;
            _Modules.CollectionPropertyChangedAsync -= OnModulePropertyChanged;
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
                if (!(e is PropertyChangedExtendedEventArgs<long> ev))
                {
                    await Task.CompletedTask;
                    return;
                }
                OnModuleCountChanged(module, ev.OldValue);
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
                OnModulesAdded(_Modules);
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
        /// <param name="module">変更があったモジュール</param>
        /// <param name="prevModuleCount">前回値モジュール数</param>
        private void OnModuleCountChanged(ModulesGridItem module, long prevModuleCount)
        {
            ModulesGridItem[] modules = { module };

            Dictionary<string, List<StorageDetailsListItem>> storageModules = AggregateStorage(modules);

            foreach (var kvp in storageModules)
            {
                // 変更対象のウェアを検索
                Storages.Where(x => x.TransportType.TransportTypeID == kvp.Key).FirstOrDefault()?.SetDetails(kvp.Value, prevModuleCount);
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
                sqlParam.Add("moduleID", DbType.String, module.ModuleID);
                sqlParam.Add("count", DbType.Int32, module.Count);
            }

            // 容量をタイプ別に集計
            DBConnection.X4DB.ExecQuery(query, sqlParam, SumStorage, modulesDict);

            return modulesDict;
        }



        /// <summary>
        /// 保管庫の容量を集計
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// args[0] = Dictionary<string, List<StorageDetailsListItem>> : 保管庫情報集計用ディクショナリ
        /// </remarks>
        private void SumStorage(IDataReader dr, object[] args)
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
                itm.ModuleCount += moduleCount;
            }
            else
            {
                // 新規追加
                modulesDict[transportTypeID].Add(new StorageDetailsListItem(moduleID, moduleCount));
            }
        }
    }
}
