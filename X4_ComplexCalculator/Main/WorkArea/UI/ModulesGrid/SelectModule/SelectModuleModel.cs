using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.EditStatus;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule
{
    class SelectModuleModel : IDisposable
    {
        #region メンバ
        /// <summary>
        /// モジュール追加先
        /// </summary>
        private ObservableRangeCollection<ModulesGridItem> ItemCollection;
        #endregion

        #region プロパティ
        /// <summary>
        /// モジュール種別
        /// </summary>
        public ObservablePropertyChangedCollection<ModulesListItem> ModuleTypes { get; } = new ObservablePropertyChangedCollection<ModulesListItem>();


        /// <summary>
        /// モジュール所有派閥
        /// </summary>
        public ObservablePropertyChangedCollection<FactionsListItem> ModuleOwners { get; } = new ObservablePropertyChangedCollection<FactionsListItem>();


        /// <summary>
        /// モジュール一覧
        /// </summary>
        public ObservablePropertyChangedCollection<ModulesListItem> Modules { get; } = new ObservablePropertyChangedCollection<ModulesListItem>();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="itemCollection">選択結果格納先</param>
        public SelectModuleModel(ObservableRangeCollection<ModulesGridItem> itemCollection)
        {
            ItemCollection = itemCollection;

            ModuleOwners.CollectionPropertyChanged += UpdateModules;
            ModuleTypes.CollectionPropertyChanged += UpdateModules;

            InitModuleTypes();
            InitModuleOwners();
            UpdateModulesMain();
        }


        /// <summary>
        /// リソースを開放
        /// </summary>
        public void Dispose()
        {
            ModuleTypes.CollectionPropertyChanged -= UpdateModules;
            ModuleOwners.CollectionPropertyChanged -= UpdateModules;
        }


        /// <summary>
        /// モジュール種別一覧を初期化する
        /// </summary>
        private void InitModuleTypes()
        {
            var items = new List<ModulesListItem>();

            void init(SQLiteDataReader dr, object[] args)
            {
                bool chked = 0 < SettingDatabase.Instance.ExecQuery($"SELECT * FROM SelectModuleCheckStateModuleTypes WHERE ID = '{dr["ModuleTypeID"]}'", (_, _) => { });
                items.Add(new ModulesListItem((string)dr["ModuleTypeID"], (string)dr["Name"], chked));
            }

            X4Database.Instance.ExecQuery(@"
SELECT
    ModuleTypeID,
    Name
from
    ModuleType
WHERE
    ModuleTypeID IN (select ModuleTypeID FROM Module)

ORDER BY Name", init, "SelectModuleCheckStateTypes");

            ModuleTypes.AddRange(items);
        }


        /// <summary>
        /// 派閥一覧を初期化する
        /// </summary>
        private void InitModuleOwners()
        {
            var items = new List<FactionsListItem>();

            void init(SQLiteDataReader dr, object[] args)
            {
                bool isChecked = 0 < SettingDatabase.Instance.ExecQuery($"SELECT * FROM SelectModuleCheckStateModuleOwners WHERE ID = '{dr["FactionID"]}'", (_, _) => { });

                var faction = Faction.Get((string)dr["FactionID"]);
                if (faction is not null) items.Add(new FactionsListItem(faction, isChecked));
            }

            X4Database.Instance.ExecQuery(@"
SELECT
    FactionID,
    Name
FROM
    Faction
WHERE
    FactionID IN (SELECT FactionID FROM WareOwner)
ORDER BY Name ASC", init, "SelectModuleCheckStateTypes");

            ModuleOwners.AddRange(items);
        }


        /// <summary>
        /// モジュール一覧を更新する
        /// </summary>
        private void UpdateModules(object sender, EventArgs e)
        {
            UpdateModulesMain();
        }


        /// <summary>
        /// モジュール一覧を更新する
        /// </summary>
        private void UpdateModulesMain()
        {
            var query = $@"
SELECT
    DISTINCT Module.ModuleID
FROM
    Module,
	WareOwner
WHERE
	Module.ModuleID = WareOwner.WareID AND
    Module.NoBlueprint = 0 AND
    Module.ModuleTypeID IN ({string.Join(", ", ModuleTypes.Where(x => x.IsChecked).Select(x => $"'{x.ID}'"))}) AND
	WareOwner.FactionID IN ({string.Join(", ", ModuleOwners.Where(x => x.IsChecked).Select(x => $"'{x.Faction.FactionID}'"))})";

            var list = new List<ModulesListItem>();

            var newModules = X4Database.Instance.Query<string>(query)
                .Select(x => new ModulesListItem(Ware.Get(x)));
                
            Modules.Reset(newModules);
        }


        /// <summary>
        /// 選択中のモジュール一覧をコレクションに追加する
        /// </summary>
        public void AddSelectedModuleToItemCollection()
        {
            // 選択されているアイテムを追加
            var items = Modules.Where(x => x.IsChecked)
                               .Select(x =>Ware.TryGet<Module>(x.ID))
                               .Where(x => x is not null)
                               .Select(x => new ModulesGridItem(x!) { EditStatus = EditStatus.Edited });

            ItemCollection.AddRange(items);
        }

        /// <summary>
        /// チェック状態を保存する
        /// </summary>
        public void SaveCheckState() => SettingDatabase.Instance.BeginTransaction(db =>
        {
            // 前回値クリア
            db.Execute("DELETE FROM SelectModuleCheckStateModuleTypes");
            db.Execute("DELETE FROM SelectModuleCheckStateModuleOwners");

            // モジュール種別のチェック状態保存
            var checkedTypes = ModuleTypes.Where(x => x.IsChecked);
            db.Execute("INSERT INTO SelectModuleCheckStateModuleTypes(ID) VALUES (:ID)", checkedTypes);

            // 派閥一覧のチェック状態保存
            var checkedFactions = ModuleOwners.Where(x => x.IsChecked).Select(x => x.Faction);
            db.Execute("INSERT INTO SelectModuleCheckStateModuleOwners(ID) VALUES (:FactionID)", checkedFactions);
        });
    }
}
