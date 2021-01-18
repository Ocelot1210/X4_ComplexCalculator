using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Prism.Mvvm;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Entity;
using Dapper;

namespace X4_ComplexCalculator.Main.Menu.File.Import.LoadoutImport
{
    /// <summary>
    /// 装備一覧アイテム1レコード分
    /// </summary>
    public class LoadoutItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// チェックされたか
        /// </summary>
        bool _IsChecked;

        /// <summary>
        /// インポート済みか
        /// </summary>
        bool _Imported;
        #endregion


        #region プロパティ
        /// <summary>
        /// 装備名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// マクロ名に対応するモジュール
        /// </summary>
        public Module Module { get; }


        /// <summary>
        /// 装備
        /// </summary>
        public WareEquipmentManager Equipment { get; }


        /// <summary>
        /// チェックされたか
        /// </summary>
        public bool IsChecked
        {
            get => _IsChecked && !Imported;     // インポート済みの場合、必ずfalseになるようにする
            set
            {
                // インポート済でない場合のみ変更許可
                if (!Imported)
                {
                    SetProperty(ref _IsChecked, value);
                }
            }
        }

        /// <summary>
        /// インポート済みか
        /// </summary>
        public bool Imported
        {
            get => _Imported;
            set
            {
                // インポートされた直後の場合、チェックを外す
                if (SetProperty(ref _Imported, value) && value)
                {
                    RaisePropertyChanged(nameof(IsChecked));
                }
            }
        }

        /// <summary>
        /// 装備中のタレットの個数
        /// </summary>
        public int TurretsCount { get; }


        /// <summary>
        /// タレットのツールチップ文字列
        /// </summary>
        public string TurretsToolTip { get; }


        /// <summary>
        /// 装備中のシールドの個数
        /// </summary>
        public int ShieldsCount { get; }


        /// <summary>
        /// シールドのツールチップ文字列
        /// </summary>
        public string ShieldsToolTip { get; }
        #endregion


        /// <summary>
        /// コンストラクタの代わり
        /// </summary>
        /// <param name="elm"></param>
        /// <returns>対応するマクロが無ければnull あればそのオブジェクト</returns>
        public static LoadoutItem? Create(XElement elm)
        {
            string macro = "";

            X4Database.Instance.ExecQuery($"SELECT Macro FROM Module WHERE Macro = '{elm.Attribute("macro").Value}'", (dr, _) =>
            {
                macro = (string)dr["Macro"];
            });

            if (string.IsNullOrEmpty(macro))
            {
                return null;
            }

            string moduleID = "";
            X4Database.Instance.ExecQuery($"SELECT ModuleID FROM Module WHERE Macro = '{macro}'", (dr, _) =>
            {
                moduleID = (string)dr["ModuleID"];
            });

            var module = Ware.TryGet<Module>(moduleID);
            if (module is null)
            {
                return null;
            }

            return new LoadoutItem(elm, module);
        }




        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="elm">装備1つ分</param>
        /// <param name="module">モジュール</param>
        private LoadoutItem(XElement elm, Module module)
        {
            Name = elm.Attribute("name").Value;

            Module = module;
            Equipment = new WareEquipmentManager(module);

            AddEquipment(elm.XPathSelectElements("groups/shields"));
            AddEquipment(elm.XPathSelectElements("groups/turrets"));


            // 同一の内容がプリセット一覧に無ければインポート可能にする
            {
                var currentEquipmentIds = Equipment.AllEquipments.Select(x => x.ID).OrderBy(x => x).ToArray();

                SettingDatabase.Instance.ExecQuery($"SELECT * FROM ModulePresets WHERE ModuleID = '{module.ID}' AND PresetName = '{Name}'", (dr1, _) =>
                {
                    var eq = new List<Equipment>();

                    SettingDatabase.Instance.ExecQuery($"SELECT EquipmentID FROM ModulePresetsEquipment WHERE ModuleID = '{module.ID}' AND PresetID = {(long)dr1["PresetID"]}", (dr2, _) =>
                    {
                        var eqp = Ware.Get<Equipment>((string)dr2["EquipmentID"]);
                        if (eqp is not null)
                        {
                            eq.Add(eqp);
                        }
                    });

                    Imported |= currentEquipmentIds.SequenceEqual(eq.Select(x => x.ID).OrderBy(x => x));
                });
            }

            var turrets     = Equipment.AllEquipments.Where(x => x.EquipmentType.EquipmentTypeID == "turrets").ToArray();
            TurretsCount    = turrets.Length;
            TurretsToolTip  = MakeEquipmentToolTipString(turrets);

            var shields     = Equipment.AllEquipments.Where(x => x.EquipmentType.EquipmentTypeID == "shields").ToArray();
            ShieldsCount    = Equipment.AllEquipments.Count(x => x.EquipmentType.EquipmentTypeID == "shields");
            ShieldsToolTip  = MakeEquipmentToolTipString(shields);
        }


        /// <summary>
        /// 装備を追加
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="manager"></param>
        private void AddEquipment(IEnumerable<XElement> elements)
        {
            foreach (var elm in elements)
            {
                const string sql = "SELECT EquipmentID FROM Equipment WHERE MacroName = :MacroName'";

                var equipmentID = X4Database.Instance.QuerySingle<string>(sql, new { MacroName = elm.Attribute("macro")?.Value ?? "" });

                var max = int.Parse(elm.Attribute("exact")?.Value ?? "1");
                var eqp = Ware.Get<Equipment>(equipmentID);
                if (eqp is not null)
                {
                    Equipment.AddEquipment(eqp, max);
                }
            }
        }


        /// <summary>
        /// インポート実行
        /// </summary>
        public bool Import()
        {
            bool ret = true;
            var id = SettingDatabase.Instance.GetLastModulePresetsID(Module.ID);

            try
            {
                SettingDatabase.Instance.BeginTransaction();

                {
                    const string sql1 = "INSERT INTO ModulePresets(ModuleID, PresetID, PresetName) VALUES(@ModuleID, @PresetID, @PresetName)";
                    var param1 = new SQLiteCommandParameters(3);
                    param1.Add("ModuleID", DbType.String, Module.ID);
                    param1.Add("PresetID", DbType.Int64, id);
                    param1.Add("PresetName", DbType.String, Name);

                    SettingDatabase.Instance.ExecQuery(sql1, param1);
                }


                var param = new SQLiteCommandParameters(4);
                foreach (var eqp in Equipment.AllEquipments)
                {
                    param.Add("moduleID", DbType.String, Module.ID);
                    param.Add("presetID", DbType.Int32, id);
                    param.Add("equipmentID", DbType.String, eqp.ID);
                    param.Add("equipmentType", DbType.String, eqp.EquipmentType.EquipmentTypeID);
                }
                SettingDatabase.Instance.ExecQuery($"INSERT INTO ModulePresetsEquipment(ModuleID, PresetID, EquipmentID, EquipmentType) VALUES(:moduleID, :presetID, :equipmentID, :equipmentType)", param);

                SettingDatabase.Instance.Commit();
                Imported = true;
            }
            catch
            {
                SettingDatabase.Instance.Rollback();
                ret = false;
            }

            return ret;
        }


        /// <summary>
        /// 装備のツールチップ文字列を作成
        /// </summary>
        /// <returns></returns>
        private string MakeEquipmentToolTipString(IEnumerable<Ware> wares)
        {
            var sb = new StringBuilder();

            var cnt = 1;
            foreach (var ware in wares.OrderBy(x => x.Name))
            {
                sb.AppendLine($"{cnt++:D2} ： {ware.Name}");
            }

            if (sb.Length == 0)
            {
                sb.Append((string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("Lang:NotEquippedToolTipText", null, null));
            }

            return sb.ToString();
        }
    }
}
