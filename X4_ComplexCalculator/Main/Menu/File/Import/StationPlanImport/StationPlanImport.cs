using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.PlanningArea;
using X4_ComplexCalculator.Main.PlanningArea.UI.ModulesGrid;
using X4_ComplexCalculator.DB.X4DB;
using System.Linq;
using Prism.Mvvm;
using WPFLocalizeExtension.Engine;

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
        private List<StationPlanItem> _PlanItems = new List<StationPlanItem>();

        /// <summary>
        /// インポート対象計画要素番号
        /// </summary>
        private int _PlanIdx = 0;
        #endregion

        #region プロパティ
        /// <summary>
        /// メニュー表示用タイトル
        /// </summary>
        public string Title { get; private set; }


        /// <summary>
        /// Viewより呼ばれるCommand
        /// </summary>
        public ICommand Command { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="command">Viewより呼ばれるCommand</param>
        public StationPlanImport(ICommand command)
        {
            Command = command;
            Title = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:ExistingPlan", null, null);
            LocalizeDictionary.Instance.PropertyChanged += Instance_PropertyChanged;
        }


        /// <summary>
        /// 選択言語変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalizeDictionary.Instance.Culture))
            {
                Title = (string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:ExistingPlan", null, null);
                RaisePropertyChanged(nameof(Title));
            }
        }


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
        /// <param name="PlanningArea"></param>
        /// <returns></returns>
        public bool Import(IPlanningArea PlanningArea)
        {
            bool ret;
            try
            {
                ret = ImportMain(PlanningArea, _PlanItems[_PlanIdx]);
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
        /// <param name="PlanningArea"></param>
        /// <param name="planItem"></param>
        /// <returns></returns>
        private bool ImportMain(IPlanningArea PlanningArea, StationPlanItem planItem)
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

            var modules = new List<Module>(moduleCount);

            // モジュール追加
            {
                var query = @"
SELECT
    ModuleID
FROM
    Module
WHERE
    Macro = :macro";


                DBConnection.X4DB.ExecQuery(query, modParam, (dr, _) =>
                {
                    modules.Add(new Module((string)dr["ModuleID"]));
                });
            }

            // 装備追加
            {
                var query = @"
SELECT
    EquipmentID,
    :index AS 'Index',
    :count AS Count,
    EquipmentTypeID
FROM
    Equipment
WHERE
    MacroName = :macro";

                DBConnection.X4DB.ExecQuery(query, eqParam, (dr, _) =>
                {
                    ModuleEquipmentManager mng = null;

                    switch ((string)dr["EquipmentTypeID"])
                    {
                        case "shields":
                            mng = modules[(int)(long)dr["Index"] - 1].Equipment.Shield;
                            break;

                        case "turrets":
                            mng = modules[(int)(long)dr["Index"] - 1].Equipment.Turret;
                            break;

                        default:
                            return;
                    }

                    var equipment = new Equipment((string)dr["EquipmentID"]);
                    var max = (long)dr["Count"];
                    for (var cnt = 0L; cnt < max; cnt++)
                    {
                        mng?.AddEquipment(equipment);
                    }
                });
            }


            // 同一モジュールをマージ
            var dict = new Dictionary<int, (int, Module, long)>();

            foreach (var (module, idx) in modules.Select((x, idx) => (x, idx)))
            {
                var hash = module.GetHashCode();
                if (dict.ContainsKey(hash))
                {
                    var tmp = dict[hash];
                    tmp.Item3++;
                    dict[hash] = tmp;
                }
                else
                {
                    dict.Add(hash, (idx, module, 1));
                }
            }

            // モジュール一覧に追加
            var range = dict.Select(x => (x.Value)).OrderBy(x => x.Item1).Select(x => new ModulesGridItem(x.Item2, null, x.Item3));
            PlanningArea.Modules.AddRange(range);


            PlanningArea.Title = planItem.PlanName;
            return true;
        }
    }
}
