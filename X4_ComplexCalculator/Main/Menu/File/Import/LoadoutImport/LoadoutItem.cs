using Prism.Mvvm;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;

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
        public ModuleEquipment Equipment { get; }

        
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
        public int TurretsCount => Equipment.Turret.AllEquipmentsCount;


        /// <summary>
        /// タレットのツールチップ文字列
        /// </summary>
        public string TurretsToolTip => MakeEquipmentToolTipString(Equipment.Turret);


        /// <summary>
        /// 装備中のシールドの個数
        /// </summary>
        public int ShieldsCount => Equipment.Shield.AllEquipmentsCount;


        /// <summary>
        /// シールドのツールチップ文字列
        /// </summary>
        public string ShieldsToolTip => MakeEquipmentToolTipString(Equipment.Shield);
        #endregion


        /// <summary>
        /// コンストラクタの代わり
        /// </summary>
        /// <param name="elm"></param>
        /// <returns>対応するマクロが無ければnull あればそのオブジェクト</returns>
        public static LoadoutItem? Create(XElement elm)
        {
            string macro = "";

            DBConnection.X4DB.ExecQuery($"SELECT Macro FROM Module WHERE Macro = '{elm.Attribute("macro").Value}'", (dr, _) =>
            {
                macro = (string)dr["Macro"];
            });

            if (string.IsNullOrEmpty(macro))
            {
                return null;
            }

            return new LoadoutItem(elm, macro);
        }




        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="elm">装備1つ分</param>
        /// <param name="macro">マクロ名</param>
        private LoadoutItem(XElement elm, string macro)
        {
            Name = elm.Attribute("name").Value;
            
            string moduleID = "";
            DBConnection.X4DB.ExecQuery($"SELECT ModuleID FROM Module WHERE Macro = '{macro}'", (dr, _) =>
            {
                moduleID = (string)dr["ModuleID"];
            });

            Module = Module.Get(moduleID);
            Equipment = ModuleEquipment.Get(moduleID);

            AddEquipment(elm.XPathSelectElements("groups/shields"), Equipment.Shield);
            AddEquipment(elm.XPathSelectElements("groups/turrets"), Equipment.Turret);


            // 同一の内容がプリセット一覧に無ければインポート可能にする
            {
                var currentEquipmentIds = Equipment.GetAllEquipment().Select(x => x.EquipmentID).OrderBy(x => x).ToArray();

                DBConnection.CommonDB.ExecQuery($"SELECT * FROM ModulePresets WHERE ModuleID = '{moduleID}' AND PresetName = '{Name}'", (dr1, _) =>
                {
                    var eq = new List<Equipment>();

                    DBConnection.CommonDB.ExecQuery($"SELECT EquipmentID FROM ModulePresetsEquipment WHERE ModuleID = '{moduleID}' AND PresetID = {(long)dr1["PresetID"]}", (dr2, __) =>
                    {
                        eq.Add(DB.X4DB.Equipment.Get((string)dr2["EquipmentID"]));
                    });

                    Imported |= currentEquipmentIds.SequenceEqual(eq.Select(x => x.EquipmentID).OrderBy(x => x));
                });
            }
        }


        /// <summary>
        /// 装備を追加
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="manager"></param>
        private void AddEquipment(IEnumerable<XElement> elements, ModuleEquipmentManager manager)
        {
            foreach (var elm in elements)
            {
                var id = "";

                DBConnection.X4DB.ExecQuery($"SELECT EquipmentID FROM Equipment WHERE MacroName = '{elm.Attribute("macro").Value}'", (dr, _) =>
                {
                    id = (string)dr["EquipmentID"];
                });

                var max = int.Parse(elm.Attribute("exact")?.Value ?? "1");
                for (var cnt = 0; cnt < max; cnt++)
                {
                    manager.AddEquipment(DB.X4DB.Equipment.Get(id));
                }
            }
        }


        /// <summary>
        /// インポート実行
        /// </summary>
        public bool Import()
        {
            var id = 0L;
            bool ret = true;

            var query = @$"
SELECT
    ifnull(MIN( PresetID + 1 ), 0) AS PresetID
FROM
    ModulePresets
WHERE
	ModuleID = '{Module.ModuleID}' AND
    ( PresetID + 1 ) NOT IN ( SELECT PresetID FROM ModulePresets WHERE ModuleID = '{Module.ModuleID}')";

            DBConnection.CommonDB.ExecQuery(query, (dr, _) =>
            {
                id = (long)dr["PresetID"];
            });

            try
            {
                DBConnection.CommonDB.BeginTransaction();
                DBConnection.CommonDB.ExecQuery($"INSERT INTO ModulePresets(ModuleID, PresetID, PresetName) VALUES('{Module.ModuleID}', {id}, '{Name}')");

                var param = new SQLiteCommandParameters(4);
                foreach (var eqp in Equipment.GetAllEquipment())
                {
                    param.Add("moduleID",      DbType.String, Module.ModuleID);
                    param.Add("presetID",      DbType.Int32 , id);
                    param.Add("equipmentID",   DbType.String, eqp.EquipmentID);
                    param.Add("equipmentType", DbType.String, eqp.EquipmentType.EquipmentTypeID);
                }
                DBConnection.CommonDB.ExecQuery($"INSERT INTO ModulePresetsEquipment(ModuleID, PresetID, EquipmentID, EquipmentType) VALUES(:moduleID, :presetID, :equipmentID, :equipmentType)", param);

                DBConnection.CommonDB.Commit();
                Imported = true;
            }
            catch
            {
                DBConnection.CommonDB.Rollback();
                ret = false;
            }

            return ret;
        }


        /// <summary>
        /// 装備のツールチップ文字列を作成
        /// </summary>
        /// <returns></returns>
        private string MakeEquipmentToolTipString(ModuleEquipmentManager equipmentManager)
        {
            var sb = new StringBuilder();

            foreach (var size in equipmentManager.Sizes)
            {
                var cnt = 1;

                foreach (var eq in equipmentManager.GetEquipment(size))
                {
                    if (cnt == 1)
                    {
                        if (sb.Length != 0)
                        {
                            sb.AppendLine();
                        }
                        sb.AppendLine($"【{size.Name}】");
                    }
                    sb.AppendLine($"{cnt++:D2} ： {eq.Name}");
                }
            }

            if (sb.Length == 0)
            {
                sb.Append((string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("Lang:NotEquippedToolTipText", null, null));
            }

            return sb.ToString();
        }
    }
}
