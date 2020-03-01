using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.ModulesGrid;

namespace X4_ComplexCalculator.Main.StorageGrid
{
    /// <summary>
    /// 保管庫一覧表示用DataGridViewのModel
    /// </summary>
    class StoragesGridModel
    {
        #region プロパティ
        /// <summary>
        /// ストレージ一覧
        /// </summary>
        public SmartCollection<StoragesGridItem> Storages { get; private set; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧用Model</param>
        public StoragesGridModel(ModulesGridModel moduleGridModel)
        {
            Storages = new SmartCollection<StoragesGridItem>();
            moduleGridModel.OnModulesChanged += ModuleGridModel_OnCollectionChanged;
        }


        /// <summary>
        /// モジュール数に変更があった場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModuleGridModel_OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var modules = (IEnumerable<ModulesGridItem>)sender;

            // カーゴ種別ごとの容量
            var transportDict = new Dictionary<string, long>();  // transportTypeID, Capacity

            // カーゴ種別ごとのモジュール集計用
            var modulesDict = new Dictionary<string, List<StorageDetailsListItem>>();

            // 前回値保存
            var backup = Storages.ToDictionary(x => x.TransportType.TransportTypeID, x => x.IsExpanded);

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
            foreach (var module in modules.Where(x => x.Module.ModuleType.ModuleTypeID == "storage"))
            {
                sqlParam.Add("moduleID", System.Data.DbType.String, module.Module.ModuleID);
                sqlParam.Add("count", System.Data.DbType.Int32, module.ModuleCount);
            }

            // 容量をタイプ別に集計
            DBConnection.X4DB.ExecQuery(query, sqlParam, SumStorage, transportDict, modulesDict);

            // コレクションに設定
            Storages.Reset(transportDict.Select(x =>
            {
                return backup.TryGetValue(x.Key, out bool expanded)
                    ? new StoragesGridItem(x.Key, x.Value, modulesDict[x.Key], expanded)
                    : new StoragesGridItem(x.Key, x.Value, modulesDict[x.Key]);
            }));
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
