using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.ModulesGrid;
using System.Collections.Specialized;

namespace X4_ComplexCalculator.Main.ResourcesGrid
{
    /// <summary>
    /// 建造に必要なリソースを表示するDataGridView用Model
    /// </summary>
    class ResourcesGridModel
    {
        #region プロパティ
        /// <summary>
        /// 建造に必要なリソース
        /// </summary>
        public SmartCollection<ResourcesGridItem> Resources { get; private set; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleGridModel">モジュール一覧用Model</param>
        public ResourcesGridModel(ModulesGridModel moduleGridModel)
        {
            Resources = new SmartCollection<ResourcesGridItem>();
            moduleGridModel.OnModulesChanged += UpdateResources;
        }

        /// <summary>
        /// 必要リソース更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateResources(object sender, NotifyCollectionChangedEventArgs e)
        {
            var task = System.Threading.Tasks.Task.Run(() =>
            {
                UpdateResourcesMain(sender, e);
            });
        }

        /// <summary>
        /// 必要リソース更新メイン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateResourcesMain(object sender, NotifyCollectionChangedEventArgs e)
        {
            // モジュール一覧
            var modules = (IEnumerable<ModulesGridItem>)sender;

            // 建造に必要なリソース集計用
            var resourcesDict = new Dictionary<string, long>();

            // 建造に必要なリソースを集計
            {
                var query = $@"
SELECT
    WareID,
    Amount * :count AS Amount

FROM
    ModuleResource

WHERE
    ModuleID = :moduleID";

                var sqlParam = new SQLiteCommandParameters(2);
                foreach (ModulesGridItem mgi in modules)
                {
                    sqlParam.Add("count", DbType.Int32, mgi.ModuleCount);
                    sqlParam.Add("moduleID", DbType.String, mgi.Module.ModuleID);
                }

                DBConnection.X4DB.ExecQuery(query, sqlParam, SumResource, resourcesDict);
            }


            {
                // 装備IDごとに集計
                var equipments = modules.Where(x => x.Module.Equipment.CanEquipped)
                                        .Select(x => x.Module.Equipment.GetAllEquipment().Select(y => new { ID = y.EquipmentID, Count = x.ModuleCount }))
                                        .Where(x => x.Any())
                                        .GroupBy(x => x.First().ID)
                                        .Select(x => new { ID = x.Key, Count = x.Sum(y => y.Sum(z => z.Count)) });

                if (equipments.Any())
                {
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
                }
            }

            Resources.Reset(resourcesDict.Select(x => new ResourcesGridItem(x.Key, x.Value)));
        }


        /// <summary>
        /// ステーション建造に必要なリソースを集計
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
