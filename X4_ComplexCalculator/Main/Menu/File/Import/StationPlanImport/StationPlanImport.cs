using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Xml.XPath;
using Prism.Mvvm;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
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
            var modules = new List<ModulesGridItem>((int)(double)planItem.Plan.XPathEvaluate("count(entry)"));

            foreach (var entry in planItem.Plan.XPathSelectElements("entry"))
            {
                // マクロ名を取得
                var macro = entry.Attribute("macro")?.Value ?? "";
                if (string.IsNullOrEmpty(macro))
                {
                    continue;
                }

                // マクロ名からモジュールを取得
                var module = X4Database.Instance.Ware.GetAll<IX4Module>().FirstOrDefault(x => x.Macro == macro);
                if (module is null)
                {
                    continue;
                }

                // モジュールの装備を取得
                var equipments = entry.XPathSelectElements("upgrades/groups/*")
                    .Select(x => (Macro: x.Attribute("macro")?.Value, Count: int.Parse(x.Attribute("exact")?.Value ?? "1")))
                    .Where(x => !string.IsNullOrEmpty(x.Macro))
                    .Select(x => (Equipment: X4Database.Instance.Ware.GetAll<IEquipment>().FirstOrDefault(y => y.Macro == x.Macro), x.Count))
                    .Where(x => x.Equipment is not null)
                    .Select(x => (Equipment: x.Equipment!, x.Count));

                var modulesGridItem = new ModulesGridItem(module);
                foreach (var equipment in equipments)
                {
                    modulesGridItem.Equipments.Add(equipment.Equipment, equipment.Count);
                }

                modules.Add(modulesGridItem);
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
