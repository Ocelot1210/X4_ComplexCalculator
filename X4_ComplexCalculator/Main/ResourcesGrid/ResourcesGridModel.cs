using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.ModulesGrid;
using System.Collections.Specialized;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common.Collection;
using System.ComponentModel;
using System;

namespace X4_ComplexCalculator.Main.ResourcesGrid
{
    /// <summary>
    /// 建造に必要なリソースを表示するDataGridView用Model
    /// </summary>
    class ResourcesGridModel : IDisposable
    {
        #region メンバ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        readonly ObservablePropertyChangedCollection<ModulesGridItem> Modules;
        #endregion


        #region プロパティ
        /// <summary>
        /// 建造に必要なリソース
        /// </summary>
        public ObservablePropertyChangedCollection<ResourcesGridItem> Resources { get; private set; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧</param>
        public ResourcesGridModel(ObservablePropertyChangedCollection<ModulesGridItem> modules)
        {
            Resources = new ObservablePropertyChangedCollection<ResourcesGridItem>();
            Modules = modules;
            Modules.CollectionChangedAsync += OnModulesCollectionChanged;
            Modules.CollectionPropertyChangedAsync += OnModulesPropertyChanged;
        }

        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            Resources.Clear();
            Modules.CollectionChangedAsync -= OnModulesCollectionChanged;
            Modules.CollectionPropertyChangedAsync -= OnModulesPropertyChanged;
        }

        /// <summary>
        /// モジュールのプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnModulesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // モジュール変更時のみ処理
            if (!(e.PropertyName == nameof(ModulesGridItem.Module) || 
                  e.PropertyName == nameof(ModulesGridItem.SelectedMethod) ||
                  e.PropertyName == nameof(ModulesGridItem.ModuleCount))
                  )
            {
                await Task.CompletedTask;
                return;
            }
            
            UpdateResources();
            await Task.CompletedTask;
        }

        /// <summary>
        /// モジュール一覧変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task OnModulesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
                Resources.Clear();
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 建造に必要なリソースを更新
        /// </summary>
        private void UpdateResources()
        {
            // 建造に必要なリソース集計用
            var resourcesDict = new Dictionary<string, long>();

            // モジュールの建造に必要なリソースを集計
            AggregateModuleResources(Modules, resourcesDict);

            // モジュールの装備の建造に必要なリソースを集計
            AggregateEquipmentResources(Modules, resourcesDict);

            var addTarget = new List<ResourcesGridItem>();
            foreach (var kvp in resourcesDict)
            {
                var item = Resources.Where(x => x.Ware.WareID == kvp.Key).FirstOrDefault();
                if (item != null)
                {
                    // 既にウェアが一覧にある場合
                    item.Count = kvp.Value;
                }
                else
                {
                    // ウェアが一覧にない場合
                    addTarget.Add(new ResourcesGridItem(kvp.Key, kvp.Value));
                }
            }

            Resources.AddRange(addTarget);
        }


        /// <summary>
        /// モジュールが追加された場合
        /// </summary>
        /// <param name="modules">追加されたモジュール</param>
        private void OnModulesAdded(IEnumerable<ModulesGridItem> modules)
        {
            Dictionary<string, long> resourcesDict = AggregateModule(modules);

            var addTarget = new List<ResourcesGridItem>();
            foreach (var kvp in resourcesDict)
            {
                var item = Resources.Where(x => x.Ware.WareID == kvp.Key).FirstOrDefault();
                if (item != null)
                {
                    // 既にウェアが一覧にある場合
                    item.Count = kvp.Value;
                }
                else
                {
                    // ウェアが一覧にない場合
                    addTarget.Add(new ResourcesGridItem(kvp.Key, kvp.Value));
                }
            }

            Resources.AddRange(addTarget);
        }


        /// <summary>
        /// モジュールが削除された場合
        /// </summary>
        /// <param name="modules">削除されたモジュール</param>
        private void OnModulesRemoved(IEnumerable<ModulesGridItem> modules)
        {
            Dictionary<string, long> resourcesDict = AggregateModule(modules);

            foreach (var kvp in resourcesDict)
            {
                var item = Resources.Where(x => x.Ware.WareID == kvp.Key).FirstOrDefault();
                if (item != null)
                {
                    item.Count = kvp.Value;
                }
            }

            Resources.RemoveAll(x => x.Count == 0);
        }



        /// <summary>
        /// 建造に必要な情報を集計
        /// </summary>
        /// <param name="modules">集計対象モジュール</param>
        /// <returns>集計結果</returns>
        private Dictionary<string, long> AggregateModule(IEnumerable<ModulesGridItem> modules)
        {
            // 建造に必要なリソース集計用
            var resourcesDict = new Dictionary<string, long>();

            // モジュールの建造に必要なリソースを集計
            AggregateModuleResources(Modules, resourcesDict);

            // モジュールの装備の建造に必要なリソースを集計
            AggregateEquipmentResources(Modules, resourcesDict);

            return resourcesDict;
        }


        /// <summary>
        /// モジュールの建造に必要なリソースを集計
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        /// <param name="resourcesDict">集計用辞書</param>
        /// <returns></returns>
        private void AggregateModuleResources(IEnumerable<ModulesGridItem> modules, Dictionary<string, long> resourcesDict)
        {
            var query = $@"
SELECT
    WareID,
    Amount * :count AS Amount

FROM
    ModuleResource

WHERE
    ModuleID = :moduleID AND
    Method   = :method";

            var sqlParam = new SQLiteCommandParameters(3);
            foreach (ModulesGridItem mgi in modules)
            {
                sqlParam.Add("count", DbType.Int32, mgi.ModuleCount);
                sqlParam.Add("moduleID", DbType.String, mgi.Module.ModuleID);
                sqlParam.Add("method", DbType.String, mgi.SelectedMethod.Method);
            }

            DBConnection.X4DB.ExecQuery(query, sqlParam, SumResource, resourcesDict);
        }


        /// <summary>
        /// モジュールの装備の建造に必要なリソースを集計
        /// </summary>
        /// <returns></returns>
        private void AggregateEquipmentResources(IEnumerable<ModulesGridItem> modules, Dictionary<string, long> resourcesDict)
        {
            // 装備IDごとに集計
            var equipments = modules.Where(x => x.Module.Equipment.CanEquipped)
                                    .Select(x => x.Module.Equipment.GetAllEquipment().Select(y => (ID : y.EquipmentID, Count : x.ModuleCount )))
                                    .Where(x => x.Any())
                                    .GroupBy(x => x.First().ID)
                                    .Select(x => (ID : x.Key, Count : x.Sum(y => y.Sum(z => z.Count)) ));

            var query = $@"
SELECT
    NeedWareID AS 'WareID',
    Amount * :count AS Amount

FROM
    EquipmentResource

WHERE
    EquipmentID = :equipmentID AND
	Method = 'default'";

            var sqlParam = new SQLiteCommandParameters(2);
            foreach (var (equipmentID, equipmentCount) in equipments)
            {
                sqlParam.Add("equipmentID", DbType.String, equipmentID);
                sqlParam.Add("count", DbType.Int32, equipmentCount);
            }
            DBConnection.X4DB.ExecQuery(query, sqlParam, SumResource, resourcesDict);
        }


        /// <summary>
        /// ウェアID別にリソースを集計
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// args[0] = Dictionary<string, long> : ウェア集計用ディクショナリ
        /// </remarks>
        private void SumResource(SQLiteDataReader dr, object[] args)
        {
            var dict = (Dictionary<string, long>)args[0];

            var key = (string)dr["WareID"];
            var value = (long)dr["Amount"];

            // ディクショナリ内にウェアが存在するか？
            if (dict.ContainsKey(key))
            {
                // すでにウェアが存在している場合
                dict[key] += value;
            }
            else
            {
                // 新規追加
                dict[key] = value;
            }
        }
    }
}
