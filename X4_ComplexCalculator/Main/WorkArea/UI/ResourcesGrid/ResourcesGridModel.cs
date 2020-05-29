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
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ResourcesGrid
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
        private readonly ObservablePropertyChangedCollection<ModulesGridItem> Modules;

        /// <summary>
        /// 単価保存用
        /// </summary>
        private readonly Dictionary<string, long> _UnitPriceBakDict = new Dictionary<string, long>();
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
            if (!(sender is ModulesGridItem module))
            {
                await Task.CompletedTask;
                return;
            }

            switch (e.PropertyName)
            {
                // モジュール数変更の場合
                case nameof(ModulesGridItem.ModuleCount):
                    {
                        if (e is PropertyChangedExtendedEventArgs<long> ev)
                        {
                            OnModuleCountChanged(module, ev.OldValue);
                        }
                    }
                    
                    break;

                // 装備変更の場合
                case nameof(ModulesGridItem.Module.Equipment):
                    {
                        if (e is PropertyChangedExtendedEventArgs<IEnumerable<string>> ev)
                        {
                            OnModuleEquipmentChanged(module, ev.OldValue);
                        }
                    }
                    
                    break;

                // 建造方式変更の場合
                case nameof(ModulesGridItem.SelectedMethod):
                    {
                        if (e is PropertyChangedExtendedEventArgs<ModuleProduction> ev)
                        {
                            OnModuleSelectedMethodChanged(module, ev.OldValue.Method);
                        }
                    }
                    break;

                default:
                    break;
            }


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
                // 単価保存
                foreach (var resource in Resources)
                {
                    _UnitPriceBakDict.Add(resource.Ware.WareID, resource.UnitPrice);
                }

                Resources.Clear();
            }

            await Task.CompletedTask;
        }


        /// <summary>
        /// モジュール数変更時に建造に必要なリソースを更新
        /// </summary>
        /// <param name="module">変更対象モジュール</param>
        /// <param name="prevModuleCount">モジュール数前回値</param>
        private void OnModuleCountChanged(ModulesGridItem module, long prevModuleCount)
        {
            (string ModuleID, long ModuleCount, string Method)[] modules = 
            {
                (module.Module.ModuleID, 1, module.SelectedMethod.Method) 
            };

            // モジュールの建造に必要なリソースを集計
            var resourcesDict = new Dictionary<string, long>();
            AggregateModuleResources(modules, resourcesDict);

            // モジュールの装備の建造に必要なリソースを集計
            // 装備IDごとに集計
            var equipments = module.Module.Equipment.GetAllEquipment().Select(x => (ID: x.EquipmentID, Count: 1))
                                   .GroupBy(x => x.ID)
                                   .Select(x => (ID: x.Key, Count: x.LongCount()));

            AggregateEquipmentResources(equipments, resourcesDict);

            foreach (var kvp in resourcesDict)
            {
                var itm = Resources.Where(x => x.Ware.WareID == kvp.Key).FirstOrDefault();
                if (itm != null)
                {
                    itm.Amount += kvp.Value * (module.ModuleCount - prevModuleCount);
                }
            }
        }

        /// <summary>
        /// モジュールの建造方式変更時に必要なリソースを更新
        /// </summary>
        /// <param name="module"></param>
        /// <param name="buildMethod"></param>
        private void OnModuleSelectedMethodChanged(ModulesGridItem module, string buildMethod)
        {
            // モジュールの建造に必要なリソースを集計用
            var resourcesDict = new Dictionary<string, long>();

            // 変更前モジュールの必要リソース集計
            {
                (string ModuleID, long ModuleCount, string Method)[] oldModules =
                {
                    (module.Module.ModuleID, 1, buildMethod)
                };
                AggregateModuleResources(oldModules, resourcesDict);

                // 変更前のモジュールのリソースはリソース一覧から引くため*-1する
                foreach (var key in resourcesDict.Keys.ToArray())
                {
                    resourcesDict[key] *= -1;
                }
            }

            // 変更後モジュールの必要リソース集計
            {
                (string ModuleID, long ModuleCount, string Method)[] newModules =
                {
                    (module.Module.ModuleID, 1, module.SelectedMethod.Method)
                };
                AggregateModuleResources(newModules, resourcesDict);
            }

            var addTarget = new List<ResourcesGridItem>();
            foreach (var kvp in resourcesDict)
            {
                var item = Resources.Where(x => x.Ware.WareID == kvp.Key).FirstOrDefault();
                if (item != null)
                {
                    // 既にウェアが一覧にある場合
                    item.Amount += kvp.Value * module.ModuleCount;
                }
                else
                {
                    // ウェアが一覧にない場合
                    addTarget.Add(new ResourcesGridItem(kvp.Key, kvp.Value * module.ModuleCount));
                }
            }

            Resources.AddRange(addTarget);
            Resources.RemoveAll(x => x.Amount == 0);
        }



        /// <summary>
        /// モジュールの装備変更時に建造に必要なリソースを更新
        /// </summary>
        /// <param name="module">変更対象モジュール</param>
        /// <param name="prevEquipments">前回装備</param>
        private void OnModuleEquipmentChanged(ModulesGridItem module, IEnumerable<string> prevEquipments)
        {
            // モジュールの建造に必要なリソースを集計
            var equipments = module.Module.Equipment.GetAllEquipment().GroupBy(x => x.EquipmentID)
                                                                      .Select(x => (x.Key, (long)x.Count()));

            var equipmentResources = new Dictionary<string, long>();
            AggregateEquipmentResources(equipments, equipmentResources);

            // 不要になったリソースを集計
            var wasteResources = new Dictionary<string, long>();
            AggregateEquipmentResources(prevEquipments.GroupBy(x => x).Select(x => (x.Key, (long)x.Count())), wasteResources);
            
            // 不要になったリソースの分減らす
            foreach (var kvp in wasteResources)
            {
                if (equipmentResources.ContainsKey(kvp.Key))
                {
                    equipmentResources[kvp.Key] -= kvp.Value;
                }
                else
                {
                    equipmentResources.Add(kvp.Key, -kvp.Value);
                }
            }

            var addTarget = new List<ResourcesGridItem>();
            foreach (var kvp in equipmentResources)
            {
                var item = Resources.Where(x => x.Ware.WareID == kvp.Key).FirstOrDefault();
                if (item != null)
                {
                    // 既にウェアが一覧にある場合
                    item.Amount += (kvp.Value * module.ModuleCount);
                }
                else
                {
                    // ウェアが一覧にない場合
                    addTarget.Add(new ResourcesGridItem(kvp.Key, kvp.Value * module.ModuleCount));
                }
            }

            Resources.AddRange(addTarget);
            Resources.RemoveAll(x => x.Amount == 0);
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
                    item.Amount += kvp.Value;
                }
                else
                {
                    // ウェアが一覧にない場合
                    item = new ResourcesGridItem(kvp.Key, kvp.Value);
                    if (_UnitPriceBakDict.ContainsKey(item.Ware.WareID))
                    {
                        item.UnitPrice = _UnitPriceBakDict[item.Ware.WareID];
                    }
                    addTarget.Add(item);
                }
            }

            // マージ処理以外で反応しないようにするためクリアする
            _UnitPriceBakDict.Clear();
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
                var itm = Resources.Where(x => x.Ware.WareID == kvp.Key).FirstOrDefault();
                if (itm != null)
                {
                    itm.Amount -= kvp.Value;
                }
            }

            Resources.RemoveAll(x => x.Amount == 0);
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
            AggregateModuleResources(modules.Select(x => (x.Module.ModuleID, x.ModuleCount, x.SelectedMethod.Method)), resourcesDict);

            // モジュールの装備の建造に必要なリソースを集計
            // 装備IDごとに集計
            var equipments = modules.Where(x => x.Module.Equipment.CanEquipped)
                                    .Select(x => x.Module.Equipment.GetAllEquipment().Select(y => (ID: y.EquipmentID, Count: x.ModuleCount)))
                                    .Where(x => x.Any())
                                    .GroupBy(x => x.First().ID)
                                    .Select(x => (ID: x.Key, Count: x.Sum(y => y.Sum(z => z.Count))));

            AggregateEquipmentResources(equipments, resourcesDict);

            return resourcesDict;
        }


        /// <summary>
        /// モジュールの建造に必要なリソースを集計
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        /// <param name="resourcesDict">集計用辞書</param>
        /// <returns></returns>
        private void AggregateModuleResources(IEnumerable<(string ModuleID, long ModuleCount, string Method)> modules, Dictionary<string, long> resourcesDict)
        {
            var query = $@"
SELECT
    Ware.WareID,
    Amount * :count AS Amount

FROM
    ModuleResource,
    Ware

WHERE
    Ware.WareID = ModuleResource.WareID AND
    ModuleID = :moduleID AND
    Method   = :method";

            var sqlParam = new SQLiteCommandParameters(3);
            foreach (var (ModuleID, ModuleCount, Method) in modules)
            {
                sqlParam.Add("count",    DbType.Int32,  ModuleCount);
                sqlParam.Add("moduleID", DbType.String, ModuleID);
                sqlParam.Add("method",   DbType.String, Method);
            }

            DBConnection.X4DB.ExecQuery(query, sqlParam, SumResource, resourcesDict);
        }


        /// <summary>
        /// 装備のリソース集計メイン
        /// </summary>
        /// <param name="equipments">(装備ID, 個数)</param>
        /// <param name="resourcesDict">集計結果</param>
        private void AggregateEquipmentResources(IEnumerable<(string EquipmentID, long Count)> equipments, Dictionary<string, long> resourcesDict)
        {
            var query = $@"
SELECT
	Ware.WareID,
    Amount * :count AS Amount

FROM
    EquipmentResource,
    Ware

WHERE
    Ware.WareID = EquipmentResource.NeedWareID AND
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

            var wareID  = (string)dr["WareID"];

            // ディクショナリ内にウェアが存在するか？
            if (!dict.ContainsKey(wareID))
            {
                // 新規追加
                dict.Add(wareID, 0);
            }
            
            var amount = (long)dr["Amount"];

            dict[wareID] += amount;
        }
    }
}
