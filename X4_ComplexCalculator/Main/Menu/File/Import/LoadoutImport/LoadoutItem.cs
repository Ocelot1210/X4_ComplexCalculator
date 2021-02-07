using Prism.Mvvm;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Entity;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

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
        public IX4Module Module { get; }


        /// <summary>
        /// 装備
        /// </summary>
        public EquippableWareEquipmentManager Equipment { get; }


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
        /// タレット情報
        /// </summary>
        public EquipmentsInfo TurretInfo { get; }


        /// <summary>
        /// シールド情報
        /// </summary>
        public EquipmentsInfo ShieldInfo { get; }
        #endregion


        /// <summary>
        /// コンストラクタの代わり
        /// </summary>
        /// <param name="elm"></param>
        /// <returns>対応するマクロが無ければnull あればそのオブジェクト</returns>
        public static LoadoutItem? Create(XElement elm)
        {
            var macro = elm.Attribute("macro")?.Value ?? "";
            if (string.IsNullOrEmpty(macro))
            {
                return null;
            }

            var module = X4Database.Instance.Ware.TryGetMacro<IX4Module>(macro);

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
        private LoadoutItem(XElement elm, IX4Module module)
        {
            Name = elm.Attribute("name").Value;

            Module = module;
            Equipment = new EquippableWareEquipmentManager(module);

            AddEquipment(elm.XPathSelectElements("groups/shields"));
            AddEquipment(elm.XPathSelectElements("groups/turrets"));


            // 同一の内容がプリセット一覧に無ければインポート可能にする
            {
                var currentEquipmentIds = Equipment.AllEquipments.Select(x => x.ID).OrderBy(x => x).ToArray();

                // 同一モジュールの同一名称のプリセットを取得する
                var presets = SettingDatabase.Instance.GetModulePreset(module.ID)
                    .Where(x => x.Name == Name)
                    .Select(x => new { ModuleID = Module.ID, PresetID = x.ID });

                foreach (var preset in presets)
                {
                    const string sqla = "SELECT EquipmentID FROM ModulePresetsEquipment WHERE ModuleID = :ModuleID AND PresetID = :PresetID";

                    var eqp = SettingDatabase.Instance.Query<string>(sqla, preset)
                        .Select(x => X4Database.Instance.Ware.TryGet<IEquipment>(x))
                        .Where(x => x is not null)
                        .Select(x => x!);

                    Imported |= currentEquipmentIds.SequenceEqual(eqp.Select(x => x.ID).OrderBy(x => x));
                    if (Imported)
                    {
                        break;
                    }
                }
            }

            TurretInfo = new EquipmentsInfo(Equipment, "turrets");
            ShieldInfo = new EquipmentsInfo(Equipment, "shields");
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
                var macro = elm.Attribute("macro")?.Value ?? "";
                if (string.IsNullOrEmpty(macro))
                {
                    continue;
                }

                var equipment = X4Database.Instance.Ware.TryGet<IEquipment>(macro);

                var max = int.Parse(elm.Attribute("exact")?.Value ?? "1");
                if (equipment is not null)
                {
                    Equipment.Add(equipment, max);
                }
            }
        }


        /// <summary>
        /// インポート実行
        /// </summary>
        public bool Import()
        {
            bool ret = true;
            var presetID = SettingDatabase.Instance.GetLastModulePresetsID(Module.ID);

            try
            {
                SettingDatabase.Instance.AddModulePreset(Module.ID, presetID, Name, Equipment.AllEquipments);
                Imported = true;
            }
            catch
            {
                ret = false;
            }

            return ret;
        }
    }
}
