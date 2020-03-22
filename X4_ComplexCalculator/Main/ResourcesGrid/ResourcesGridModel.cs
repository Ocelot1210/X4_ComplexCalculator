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

namespace X4_ComplexCalculator.Main.ResourcesGrid
{
    /// <summary>
    /// 建造に必要なリソースを表示するDataGridView用Model
    /// </summary>
    class ResourcesGridModel
    {
        #region メンバ
        /// <summary>
        /// モジュール一覧
        /// </summary>
        readonly IReadOnlyCollection<ModulesGridItem> Modules;
        #endregion


        #region プロパティ
        /// <summary>
        /// 建造に必要なリソース
        /// </summary>
        public MemberChangeDetectCollection<ResourcesGridItem> Resources { get; private set; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧</param>
        public ResourcesGridModel(MemberChangeDetectCollection<ModulesGridItem> modules)
        {
            Resources = new MemberChangeDetectCollection<ResourcesGridItem>();
            Modules = modules;
            modules.OnCollectionChangedAsync += OnModulesChanged;
            modules.OnPropertyChangedAsync += OnModulesPropertyChanged;
        }

        /// <summary>
        /// モジュールのプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task OnModulesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 建造に必要なリソース集計用
            var resourcesDict = new Dictionary<string, long>();

            Task.WaitAll(
                // モジュールの建造に必要なリソースを集計
                AggregateModuleResources(Modules, resourcesDict),

                // モジュールの装備の建造に必要なリソースを集計
                AggregateEquipmentResources(Modules, resourcesDict)
            );

            Resources.Reset(resourcesDict.Select(x => new ResourcesGridItem(x.Key, x.Value)).OrderBy(x => x.Ware.Name));
            await Task.CompletedTask;
        }

        /// <summary>
        /// モジュール一覧変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task OnModulesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // 建造に必要なリソース集計用
            var resourcesDict = new Dictionary<string, long>();

            Task.WaitAll(
                // モジュールの建造に必要なリソースを集計
                AggregateModuleResources(Modules, resourcesDict),

                // モジュールの装備の建造に必要なリソースを集計
                AggregateEquipmentResources(Modules, resourcesDict)
            );

            Resources.Reset(resourcesDict.Select(x => new ResourcesGridItem(x.Key, x.Value)));
            await Task.CompletedTask;
        }


        /// <summary>
        /// モジュールの建造に必要なリソースを集計
        /// </summary>
        /// <param name="modules">モジュール一覧</param>
        /// <param name="resourcesDict">集計用辞書</param>
        /// <returns></returns>
        private async Task AggregateModuleResources(IEnumerable<ModulesGridItem> modules, Dictionary<string, long> resourcesDict)
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

            await Task.CompletedTask;
        }


        /// <summary>
        /// モジュールの装備の建造に必要なリソースを集計
        /// </summary>
        /// <returns></returns>
        private async Task AggregateEquipmentResources(IEnumerable<ModulesGridItem> modules, Dictionary<string, long> resourcesDict)
        {
            // 装備IDごとに集計
            var equipments = modules.AsParallel()
                                    .Where(x => x.Module.Equipment.CanEquipped)
                                    .Select(x => x.Module.Equipment.GetAllEquipment().Select(y => new { ID = y.EquipmentID, Count = x.ModuleCount }))
                                    .Where(x => x.Any())
                                    .GroupBy(x => x.First().ID)
                                    .Select(x => new { ID = x.Key, Count = x.Sum(y => y.Sum(z => z.Count)) });

            var query = $@"
SELECT
    WareID,
    Amount * :count AS Amount

FROM
    EquipmentResource

WHERE
    EquipmentID = :equipmentID AND
	Method = 'default'";

            var sqlParam = new SQLiteCommandParameters(2);
            foreach (var equipment in equipments)
            {
                sqlParam.Add("equipmentID", DbType.String, equipment.ID);
                sqlParam.Add("count", DbType.Int32, equipment.Count);
            }
            DBConnection.X4DB.ExecQuery(query, sqlParam, SumResource, resourcesDict);

            await Task.CompletedTask;
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

            var key = dr["WareID"].ToString();
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
