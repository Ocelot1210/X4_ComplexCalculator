using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.ModulesGrid.EditEquipment
{
    /// <summary>
    /// 装備編集画面のModel
    /// </summary>
    class EditEquipmentModel : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 編集対象モジュール
        /// </summary>
        private Module _Module;


        /// <summary>
        /// 選択中のプリセット
        /// </summary>
        private PresetComboboxItem _SelectedPreset;
        #endregion

        #region プロパティ
        /// <summary>
        /// 装備サイズ一覧
        /// </summary>
        public SmartCollection<Size> EquipmentSizes { get; private set; } = new SmartCollection<Size>();


        /// <summary>
        /// 種族一覧
        /// </summary>
        public MemberChangeDetectCollection<FactionsListItem> Factions { get; } = new MemberChangeDetectCollection<FactionsListItem>();


        /// <summary>
        /// プリセット名
        /// </summary>
        public SmartCollection<PresetComboboxItem> Presets { get; } = new SmartCollection<PresetComboboxItem>();


        /// <summary>
        /// 選択中のプリセット
        /// </summary>
        public PresetComboboxItem SelectedPreset
        {
            get
            {
                return _SelectedPreset;
            }
            set
            {
                _SelectedPreset = value;
                OnPropertyChanged();
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">編集対象モジュール</param>
        public EditEquipmentModel(Module module)
        {
            // 初期化
            _Module = module;
            InitEquipmentSizes(module.ModuleID);
            UpdateFactions();
            InitPreset(module.ModuleID);
        }


        /// <summary>
        /// 装備サイズコンボボックスの内容を初期化
        /// </summary>
        /// <param name="moduleID"></param>
        private void InitEquipmentSizes(string moduleID)
        {
            static void AddItem(SQLiteDataReader dr, object[] args)
            {
                ((ICollection<Size>)args[0]).Add(new Size(dr["SizeID"].ToString()));
            }

            var sizes = new List<Size>();
            DBConnection.X4DB.ExecQuery($"SELECT DISTINCT ModuleShield.SizeID FROM ModuleShield, ModuleTurret WHERE ModuleShield.ModuleID = ModuleTurret.ModuleID AND ModuleShield.ModuleID = '{moduleID}'", AddItem, sizes);
            EquipmentSizes.AddRange(sizes);
        }


        /// <summary>
        /// 派閥一覧を更新
        /// </summary>
        private void UpdateFactions()
        {
            static void AddItem(SQLiteDataReader dr, object[] args)
            {
                bool chkState = 0 < DBConnection.CommonDB.ExecQuery($"SELECT ID FROM SelectModuleEquipmentCheckStateFactions WHERE ID = '{dr["FactionID"]}'", (SQLiteDataReader drDummy, object[] argsDummy) => { });

                ((ICollection<FactionsListItem>)args[0]).Add(new FactionsListItem(dr["FactionID"].ToString(), chkState));
            }

            var query = $@"
SELECT
	DISTINCT FactionID
FROM
	EquipmentOwner
WHERE
	EquipmentID IN (SELECT EquipmentID FROM Equipment WHERE (EquipmentTypeID IN ('turrets', 'shields')))";

            var items = new List<FactionsListItem>();
            DBConnection.X4DB.ExecQuery(query, AddItem, items);
            Factions.AddRange(items);
        }

        /// <summary>
        /// プリセットを初期化
        /// </summary>
        private void InitPreset(string moduleID)
        {
            DBConnection.CommonDB.ExecQuery($"SELECT DISTINCT PresetID, PresetName FROM ModulePresets WHERE ModuleID = '{moduleID}'", (SQLiteDataReader dr, object[] args) =>
            {
                Presets.Add(new PresetComboboxItem((long)dr["PresetID"], dr["PresetName"].ToString()));
            });
        }


        /// <summary>
        /// チェック状態を保存
        /// </summary>
        public void SaveCheckState()
        {
            // 前回値クリア
            DBConnection.CommonDB.ExecQuery("DELETE FROM SelectModuleEquipmentCheckStateFactions", null);

            // トランザクション開始
            DBConnection.CommonDB.BeginTransaction();

            // モジュール種別のチェック状態保存
            foreach (var id in Factions.Where(x => x.Checked).Select(x => x.Faction.FactionID))
            {
                DBConnection.CommonDB.ExecQuery($"INSERT INTO SelectModuleEquipmentCheckStateFactions(ID) VALUES ('{id}')", null);
            }

            // コミット
            DBConnection.CommonDB.Commit();
        }


        /// <summary>
        /// プリセット保存
        /// </summary>
        public void SavePreset()
        {
            if (SelectedPreset == null)
            {
                return;
            }

            DBConnection.CommonDB.BeginTransaction();
            var newPreset = new PresetComboboxItem(SelectedPreset.ID, SelectedPreset.Name);
            Presets.Replace(SelectedPreset, newPreset);
            DBConnection.CommonDB.Commit();
        }


        /// <summary>
        /// プリセット追加
        /// </summary>
        public void AddPreset()
        {
            var id = 0L;

            var query = @$"
SELECT
    ifnull(MIN( PresetID + 1 ), 0) AS PresetID
FROM
    ModulePresets
WHERE
	ModuleID = '{_Module.ModuleID}' AND
    ( PresetID + 1 ) NOT IN ( SELECT PresetID FROM ModulePresets WHERE ModuleID = '{_Module.ModuleID}')";

            DBConnection.CommonDB.ExecQuery(query, (SQLiteDataReader dr, object[] args) =>
            {
                id = (long)dr["PresetID"];
            });

            var item = new PresetComboboxItem(id, "新規プリセット");

            DBConnection.CommonDB.BeginTransaction();
            DBConnection.CommonDB.ExecQuery($"INSERT INTO ModulePresets(ModuleID, PresetID, PresetName) VALUES('{_Module.ModuleID}', {item.ID}, '{item.Name}')");
            Presets.Add(item);
            DBConnection.CommonDB.Commit();

            SelectedPreset = item;
        }

        /// <summary>
        /// プリセットを削除
        /// </summary>
        public void RemovePreset()
        {
            if (SelectedPreset == null)
            {
                return;
            }

            var query = $"";

            DBConnection.CommonDB.BeginTransaction();
            DBConnection.CommonDB.ExecQuery($"DELETE FROM ModulePresets WHERE ModuleID = '{_Module.ModuleID}' AND PresetID = {SelectedPreset.ID}");
            Presets.Remove(SelectedPreset);
            DBConnection.CommonDB.Commit();

            SelectedPreset = Presets.FirstOrDefault();
        }
    }
}
