using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Xml.XPath;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

namespace X4_ComplexCalculator.Main.Menu.File.Import.SaveDataImport
{
    /// <summary>
    /// X4のセーブデータからインポートする機能用クラス
    /// </summary>
    class SaveDataImport : IImport
    {
        #region メンバ
        /// <summary>
        /// インポート対象ステーション一覧
        /// </summary>
        private readonly List<SaveDataStationItem> Stations = new List<SaveDataStationItem>();


        /// <summary>
        /// インポート対象ステーション要素番号
        /// </summary>
        private int _StationIdx = 0;
        #endregion


        /// <summary>
        /// メニュー表示用タイトル
        /// </summary>
        public string Title => "X4 セーブデータ";


        /// <summary>
        /// Viewより呼ばれるCommand
        /// </summary>
        public ICommand Command { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="command">Viewより呼ばれるCommand</param>
        public SaveDataImport(ICommand command)
        {
            Command = command;
        }


        /// <summary>
        /// インポート対象を選択
        /// </summary>
        /// <returns>インポート対象数</returns>
        public int Select()
        {
            var onOK = SelectStationDialog.ShowDialog(Stations);
            if (!onOK)
            {
                Stations.Clear();
            }

            _StationIdx = 0;
            return Stations.Count;
        }


        /// <summary>
        /// インポート実行
        /// </summary>
        /// <param name="WorkArea">作業エリア</param>
        /// <returns>インポートに成功したか</returns>
        public bool Import(IWorkArea WorkArea)
        {
            bool ret;
            try
            {
                ret = ImportMain(WorkArea, Stations[_StationIdx]);
                _StationIdx++;
            }
            catch
            {
                ret = false;
            }

            return ret;
        }


        /// <summary>
        /// インポート実行メイン
        /// </summary>
        /// <param name="WorkArea"></param>
        /// <param name="saveData"></param>
        /// <returns></returns>
        private bool ImportMain(IWorkArea WorkArea, SaveDataStationItem saveData)
        {
            // モジュール一覧を設定
            SetModules(WorkArea, saveData);

            // 製品価格を設定
            SetWarePrice(WorkArea, saveData);

            // 保管庫割当状態を設定
            SetStorageAssign(WorkArea, saveData);

            WorkArea.Title = saveData.StationName;

            return true;
        }


        /// <summary>
        /// モジュール一覧を設定
        /// </summary>
        /// <param name="WorkArea"></param>
        /// <param name="saveData"></param>
        private void SetModules(IWorkArea WorkArea, SaveDataStationItem saveData)
        {
            var saveEntries = saveData.XElement.XPathSelectElements("construction/sequence/entry").ToArray();

            var modParam = saveEntries.Select(entry => new { macro = entry.Attribute("macro").Value });

            var eqParam = saveEntries.SelectMany(entry =>
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


            var modules = new List<ModulesGridItem>(saveEntries.Length);

            // モジュール追加
            {
                const string query = "SELECT ModuleID FROM Module WHERE Macro = :macro";
                var moduleIds = modParam.SelectMany(macro => DBConnection.X4DB.ExecQuery<string>(query, new { macro }));
                foreach (var moduleID in moduleIds)
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

                var results = DBConnection.X4DB.ExecQuery<(string, int, long, string)>(query, eqParam);
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
                    if (equipment == null) return;

                    var max = count;
                    for (var cnt = 0L; cnt < max; cnt++)
                    {
                        mng?.AddEquipment(equipment);
                    }
                }
            }

            // 同一モジュールをマージ
            var dict = new Dictionary<int, (int, Module, ModuleProduction, long)>();

            foreach (var (module, idx) in modules.Select((x, idx) => (x, idx)))
            {
                var hash = HashCode.Combine(module.Module, module.SelectedMethod);
                if (dict.ContainsKey(hash))
                {
                    var tmp = dict[hash];
                    tmp.Item4 += module.ModuleCount;
                    dict[hash] = tmp;
                }
                else
                {
                    dict.Add(hash, (idx, module.Module, module.SelectedMethod, module.ModuleCount));
                }
            }

            // モジュール一覧に追加
            var range = dict.Select(x => (x.Value)).OrderBy(x => x.Item1).Select(x => new ModulesGridItem(x.Item2, x.Item3, x.Item4));
            WorkArea.Modules.AddRange(range);
        }




        /// <summary>
        /// 製品価格を設定
        /// </summary>
        /// <param name="WorkArea"></param>
        /// <param name="saveData"></param>
        private void SetWarePrice(IWorkArea WorkArea, SaveDataStationItem saveData)
        {
            foreach (var ware in saveData.XElement.XPathSelectElements("/economylog/*[not(self::cargo)]"))
            {
                var wareID = ware.Attribute("ware").Value;
                var prod = WorkArea.Products.Where(x => x.Ware.WareID == wareID).FirstOrDefault();
                if (prod != null)
                {
                    prod.UnitPrice = long.Parse(ware.Attribute("price").Value);
                }
            }
        }


        /// <summary>
        /// 保管庫割当状態を設定
        /// </summary>
        /// <param name="WorkArea"></param>
        /// <param name="saveData"></param>
        private void SetStorageAssign(IWorkArea WorkArea, SaveDataStationItem saveData)
        {
            foreach (var ware in saveData.XElement.XPathSelectElements("overrides/max/ware"))
            {
                var wareID = ware.Attribute("ware").Value;

                var storage = WorkArea.StorageAssign.Where(x => x.WareID == wareID).FirstOrDefault();
                if (storage != null)
                {
                    var amount = long.Parse(ware.Attribute("amount").Value);

                    storage.AllocCount = amount;
                }
            }
        }
    }
}
