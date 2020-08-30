﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Xml.XPath;
using Prism.Mvvm;
using WPFLocalizeExtension.Engine;
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
            var planEntries = planItem.Plan.XPathSelectElements("entry").ToArray();

            var modParam = planEntries.Select(entry => entry.Attribute("macro").Value);

            var eqParam = planEntries.SelectMany(entry =>
            {
                var index = int.Parse(entry.Attribute("index").Value);
                var equipmets = entry.XPathSelectElements("upgrades/groups/*");
                return equipmets.Select(equipmet => (index, equipmet));
            }).Select(args =>
            {
                var (index, equipmet) = args;
                var macro = equipmet.Attribute("macro").Value;
                var count = int.Parse(equipmet.Attribute("exact")?.Value ?? "1");
                return new { index, macro, count };
            });

            var modules = new List<ModulesGridItem>(planEntries.Length);

            // モジュール追加
            {
                const string query = "SELECT ModuleID FROM Module WHERE Macro IN :modParam";
                foreach (var moduleID in DBConnection.X4DB.ExecQuery<string>(query, new { modParam }))
                {
                    var module = Module.Get(moduleID);
                    if (module != null)
                    {
                        modules.Add(new ModulesGridItem(module));
                    }
                }
            }

            // 装備追加
            {
                const string query = @"
SELECT
    EquipmentID,
    :index AS 'Index',
    :count AS Count,
    EquipmentTypeID
FROM
    Equipment
WHERE
    MacroName = :macro";

                var results = eqParam.SelectMany(param => DBConnection.X4DB.ExecQuery<(string, int, long, string)>(query, param));
                foreach (var (equipmentID, index, count, equipmentTypeID) in results)
                {
                    ModuleEquipmentManager? mng = null;

                    switch (equipmentTypeID)
                    {
                        case "shields":
                            mng = modules[index - 1].ModuleEquipment.Shield;
                            break;

                        case "turrets":
                            mng = modules[index - 1].ModuleEquipment.Turret;
                            break;

                        default:
                            continue;
                    }

                    var equipment = Equipment.Get(equipmentID);
                    if (equipment == null) continue;
                    var max = count;
                    for (var cnt = 0L; cnt < max; cnt++)
                    {
                        mng?.AddEquipment(equipment);
                    }
                }
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
            WorkArea.Modules.AddRange(dict.Select(x => x.Value).OrderBy(x => x.Module.Name));

            // 編集状態を全て未編集にする
            IEnumerable<IEditable>[] editables = { WorkArea.Modules, WorkArea.Products, WorkArea.Resources, WorkArea.StorageAssign };
            foreach (var editable in editables.SelectMany(x => x))
            {
                editable.EditStatus = EditStatus.Unedited;
            }

            WorkArea.Title = planItem.PlanName;
            return true;
        }
    }
}
