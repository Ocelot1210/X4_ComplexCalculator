using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.ModulesGrid;

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
            moduleGridModel.OnModulesChanged += ModuleGridModel_OnCollectionChanged;
        }


        /// <summary>
        /// モジュール一覧変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModuleGridModel_OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Resources.Clear();

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


            // TODO: モジュールの装備の分も集計
            {
                var equipmentsDict = new Dictionary<string, int>();     // 装備ID, 個数

                // 装備IDごとに集計
                foreach (var moduleGridItem in modules.Where(x => x.Module.Equipment.CanEquipped))
                {
                    ModuleEquipmentManager[] mng = { moduleGridItem.Module.Equipment.Turret, moduleGridItem.Module.Equipment.Shield };

                    foreach (var equipments in mng.Select(x => x.AllEquipments.GroupBy(y => y.EquipmentID).Select(y => new { ID = y.Key, Count = y.Count() })))
                    {
                        foreach (var equipment in equipments)
                        {
                            if (equipmentsDict.ContainsKey(equipment.ID))
                            {
                                equipmentsDict[equipment.ID] += equipment.Count * moduleGridItem.ModuleCount;
                            }
                            else
                            {
                                equipmentsDict[equipment.ID] = equipment.Count * moduleGridItem.ModuleCount;
                            }
                        }
                    }
                }

                var query = $@"
SELECT
    WareID,
    Amount * :count AS Amount

FROM
    EquipmentResource

WHERE
    EquipmentID = :moduleID AND
	Method = 'default'";

                var sqlParam = new SQLiteCommandParameters(2);
                foreach (var equipment in equipmentsDict)
                {
                    sqlParam.Add("count", DbType.Int32, equipment.Value);
                    sqlParam.Add("moduleID", DbType.String, equipment.Key);
                }

                DBConnection.X4DB.ExecQuery(query, sqlParam, SumResource, resourcesDict);
            }

            Resources.AddRange(resourcesDict.Select(x => new ResourcesGridItem(x.Key, x.Value)));
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
