using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.PlanningArea;
using X4_ComplexCalculator.Main.PlanningArea.UI.ModulesGrid;

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
        /// <param name="PlanningArea">作業エリア</param>
        /// <returns>インポートに成功したか</returns>
        public bool Import(IPlanningArea PlanningArea)
        {
            bool ret;
            try
            {
                ret = ImportMain(PlanningArea, Stations[_StationIdx]);
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
        /// <param name="PlanningArea"></param>
        /// <param name="saveData"></param>
        /// <returns></returns>
        private bool ImportMain(IPlanningArea PlanningArea, SaveDataStationItem saveData)
        {
            // モジュール一覧を設定
            SetModules(PlanningArea, saveData);

            // 製品価格を設定
            SetWarePrice(PlanningArea, saveData);

            // 保管庫割当状態を設定
            SetStorageAssign(PlanningArea, saveData);

            PlanningArea.Title = saveData.StationName;

            return true;
        }


        /// <summary>
        /// モジュール一覧を設定
        /// </summary>
        /// <param name="PlanningArea"></param>
        /// <param name="saveData"></param>
        private void SetModules(IPlanningArea PlanningArea, SaveDataStationItem saveData)
        {
            var modParam = new SQLiteCommandParameters(1);
            var eqParam = new SQLiteCommandParameters(3);
            var moduleCount = 0;

            foreach (var entry in saveData.XElement.XPathSelectElements("construction/sequence/entry"))
            {
                var index = int.Parse(entry.Attribute("index").Value);
                modParam.Add("macro", System.Data.DbType.String, entry.Attribute("macro").Value);

                foreach (var equipmet in entry.XPathSelectElements("upgrades/groups/*"))
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

            // 重複削除
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
        }




        /// <summary>
        /// 製品価格を設定
        /// </summary>
        /// <param name="PlanningArea"></param>
        /// <param name="saveData"></param>
        private void SetWarePrice(IPlanningArea PlanningArea, SaveDataStationItem saveData)
        {
            foreach (var ware in saveData.XElement.XPathSelectElements("/economylog/*[not(self::cargo)]"))
            {
                var wareID = ware.Attribute("ware").Value;
                var prod = PlanningArea.Products.Where(x => x.Ware.WareID == wareID).FirstOrDefault();
                if (prod != null)
                {
                    prod.UnitPrice = long.Parse(ware.Attribute("price").Value);
                }
            }
        }


        /// <summary>
        /// 保管庫割当状態を設定
        /// </summary>
        /// <param name="PlanningArea"></param>
        /// <param name="saveData"></param>
        private void SetStorageAssign(IPlanningArea PlanningArea, SaveDataStationItem saveData)
        {
            foreach (var ware in saveData.XElement.XPathSelectElements("overrides/max/ware"))
            {
                var wareID = ware.Attribute("ware").Value;

                var storage = PlanningArea.StorageAssign.Where(x => x.WareID == wareID).FirstOrDefault();
                if (storage != null)
                {
                    var amount = long.Parse(ware.Attribute("amount").Value);

                    storage.AllocCount = amount;
                }
            }
        }
    }
}
