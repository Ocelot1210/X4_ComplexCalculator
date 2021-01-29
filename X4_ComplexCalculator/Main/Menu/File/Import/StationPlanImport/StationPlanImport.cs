using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Xml.XPath;
using Prism.Mvvm;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.Menu.File.Import.StationPlanImport
{
    /// <summary>
    /// 既存の計画ファイルからインポートする
    /// </summary>
    class StationPlanImport : BindableBase, IImport
    {
        #region メンバ
        /// <summary>
        /// インポート対象計画一覧
        /// </summary>
        private List<StationPlanItem> _PlanItems = new();

        /// <summary>
        /// インポート対象計画要素番号
        /// </summary>
        private int _PlanIdx = 0;
        #endregion


        #region プロパティ
        /// <summary>
        /// メニュー表示用タイトル
        /// </summary>
        public string Title => "Lang:ExistingPlan";


        /// <summary>
        /// Viewより呼ばれるCommand
        /// </summary>
        public ICommand Command { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="command">Viewより呼ばれるCommand</param>
        public StationPlanImport(ICommand command) => Command = command;


        /// <summary>
        /// インポート対象を選択
        /// </summary>
        /// <returns>インポート対象数</returns>
        public int Select()
        {
            _PlanItems.Clear();

            bool onOK = SelectPlanDialog.ShowDialog(_PlanItems);
            if (!onOK)
            {
                _PlanItems.Clear();
            }

            _PlanIdx = 0;
            return _PlanItems.Count;
        }


        /// <summary>
        /// インポート処理
        /// </summary>
        /// <param name="WorkArea"></param>
        /// <returns></returns>
        public bool Import(IWorkArea WorkArea)
        {
            bool ret;
            try
            {
                ret = ImportMain(WorkArea, _PlanItems[_PlanIdx]);
                _PlanIdx++;
            }
            catch
            {
                ret = false;
            }

            return ret;
        }


        /// <summary>
        /// インポートメイン処理
        /// </summary>
        /// <param name="WorkArea"></param>
        /// <param name="planItem"></param>
        /// <returns></returns>
        private bool ImportMain(IWorkArea WorkArea, StationPlanItem planItem)
        {
            var modParam = new SQLiteCommandParameters(1);
            var eqParam = new SQLiteCommandParameters(3);
            var moduleCount = 0;

            foreach (var module in planItem.Plan.XPathSelectElements("entry"))
            {
                var index = int.Parse(module.Attribute("index").Value);
                modParam.Add("macro", System.Data.DbType.String, module.Attribute("macro").Value);

                foreach (var equipmet in module.XPathSelectElements("upgrades/groups/*"))
                {
                    eqParam.Add("index", System.Data.DbType.Int32, index);
                    eqParam.Add("macro", System.Data.DbType.String, equipmet.Attribute("macro").Value);
                    eqParam.Add("count", System.Data.DbType.Int32, int.Parse(equipmet.Attribute("exact")?.Value ?? "1"));
                }

                moduleCount++;
            }

            var modules = new List<ModulesGridItem>(moduleCount);

            // モジュール追加
            {
                var query = @"
SELECT
    ModuleID
FROM
    Module
WHERE
    Macro = :macro";

                var tmpModules = X4Database.Instance.Query<string>(query, modParam)
                    .Select(x => Ware.TryGet<Module>(x))
                    .Where(x => x is not null)
                    .Select(x => new ModulesGridItem(x!));
                modules.AddRange(tmpModules);
            }

            // 装備追加
            {
                var query = @"
SELECT
    EquipmentID,
    :index AS 'Index',
    :count AS Count
FROM
    Equipment
WHERE
    MacroName = :macro";

                X4Database.Instance.ExecQuery(query, eqParam, (dr, _) =>
                {
                    var index = (int)(long)dr["Index"] - 1;
                    var moduleEquipment = modules[index].Equipments;

                    var equipment = Ware.TryGet<Equipment>((string)dr["EquipmentID"]);
                    if (equipment is null) return;

                    var count = (long)dr["Count"];
                    moduleEquipment.Add(equipment, count);
                });
            }


            // 同一モジュールをマージ
            var dict = new Dictionary<int, ModulesGridItem>();

            foreach (var (module, idx) in modules.Select((x, idx) => (x, idx)))
            {
                var hash = module.GetHashCode();
                if (dict.ContainsKey(hash))
                {
                    dict[hash].ModuleCount += module.ModuleCount;
                }
                else
                {
                    dict.Add(hash, module);
                }
            }

            // モジュール一覧に追加
            WorkArea.StationData.ModulesInfo.Modules.AddRange(dict.Select(x => x.Value).OrderBy(x => x.Module.Name));

            // 編集状態を全て未編集にする
            IEnumerable<IEditable>[] editables =
            {
                WorkArea.StationData.ModulesInfo.Modules,
                WorkArea.StationData.ProductsInfo.Products,
                WorkArea.StationData.BuildResourcesInfo.BuildResources,
                WorkArea.StationData.StorageAssignInfo.StorageAssign,
            };
            foreach (var editable in editables.SelectMany(x => x))
            {
                editable.EditStatus = EditStatus.Unedited;
            }

            WorkArea.Title = planItem.PlanName;
            return true;
        }
    }
}
