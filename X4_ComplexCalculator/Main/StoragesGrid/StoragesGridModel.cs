using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.ModulesGrid;
using System.Collections.Specialized;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common.Collection;
using System;

namespace X4_ComplexCalculator.Main.StoragesGrid
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
            Modules.OnCollectionChangedAsync += OnModulesChanged;
            Modules.OnCollectionPropertyChangedAsync += OnModulePropertyChanged;
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            Modules.OnCollectionChangedAsync -= OnModulesChanged;
            Modules.OnCollectionPropertyChangedAsync -= OnModulePropertyChanged;
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
                UpdateStorages();
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
            UpdateStorages();
            await Task.CompletedTask;
        }


        /// <summary>
        /// 保管庫情報更新
        /// </summary>
        private void UpdateStorages()
        {
            var modules = Modules.Where(x => x.Module.ModuleType.ModuleTypeID == "storage")
                                 .GroupBy(x => x.Module.ModuleID)
                                 .Select(x => new { x.First().Module.ModuleID, Count = x.Sum(y => y.ModuleCount) });

            // 処理対象が無ければクリアして終わる
            if (!modules.Any())
            {
                Storages.Clear();
                return;
            }

            var sqlParam = new SQLiteCommandParameters(2);

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

            // 保管モジュールのみ列挙)
            foreach (var module in modules)
            {
                sqlParam.Add("moduleID", System.Data.DbType.String, module.ModuleID);
                sqlParam.Add("count", System.Data.DbType.Int32, module.Count);
            }


            // カーゴ種別ごとの容量
            var transportDict = new Dictionary<string, long>();  // transportTypeID, Capacity

            // カーゴ種別ごとのモジュール集計用
            var modulesDict = new Dictionary<string, List<StorageDetailsListItem>>();

            // 容量をタイプ別に集計
            DBConnection.X4DB.ExecQuery(query, sqlParam, SumStorage, transportDict, modulesDict);

            // 前回値保存
            var backup = Storages.ToDictionary(x => x.TransportType.TransportTypeID, x => x.IsExpanded);

            // コレクションに設定
            Storages.Reset(transportDict.Select(x =>
            {
                var details = modulesDict[x.Key].OrderBy(x => x.ModuleName).ToArray();
                return backup.TryGetValue(x.Key, out bool expanded)
                    ? new StoragesGridItem(x.Key, x.Value, details, expanded)
                    : new StoragesGridItem(x.Key, x.Value, details);
            })
                .OrderBy(x => x.TransportType.Name)
            );
        }

        /// <summary>
        /// 保管庫の容量を集計
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// args[0] = Dictionary<string, int> : ウェア集計用ディクショナリ
        /// </remarks>
        private void SumStorage(SQLiteDataReader dr, object[] args)
        {
            var transportDict = (Dictionary<string, long>)args[0];     // transportTypeID, Capacity

            var transportTypeID = dr["TransportTypeID"].ToString();
            var amount          = (long)dr["Amount"];

            // ディクショナリ内にウェアが存在するか？
            if (transportDict.ContainsKey(transportTypeID))
            {
                // すでにウェアが存在している場合
                transportDict[transportTypeID] += amount;
            }
            else
            {
                // 新規追加
                transportDict[transportTypeID] = amount;
            }


            // 関連モジュール集計
            var modulesDict = (Dictionary<string, List<StorageDetailsListItem>>)args[1];

            // このカーゴ種別に対して関連モジュール追加が初回か？
            if (!modulesDict.ContainsKey(transportTypeID))
            {
                modulesDict.Add(transportTypeID, new List<StorageDetailsListItem>());
            }

            var moduleID = dr["ModuleID"].ToString();
            var moduleCount = (long)dr["Count"];

            // このカーゴ種別に対し、既にモジュールが追加されているか？
            var itm = modulesDict[transportTypeID].Where(x => x.ModuleID == moduleID);
            if (itm.Any())
            {
                // 既にモジュールが追加されている場合、モジュール数を増やしてレコードがなるべく少なくなるようにする
                itm.First().Incriment(moduleCount);
            }
            else
            {
                // 新規追加
                modulesDict[transportTypeID].Add(new StorageDetailsListItem(moduleID, moduleCount));
            }
        }
    }
}
