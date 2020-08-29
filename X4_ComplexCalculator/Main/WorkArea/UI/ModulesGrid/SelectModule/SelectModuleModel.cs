using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
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
        public ObservablePropertyChangedCollection<ModulesListItem> ModuleTypes { get; private set; } = new ObservablePropertyChangedCollection<ModulesListItem>();


        /// <summary>
        /// モジュール所有派閥
        /// </summary>
        public ObservablePropertyChangedCollection<FactionsListItem> ModuleOwners { get; private set; } = new ObservablePropertyChangedCollection<FactionsListItem>();


        /// <summary>
        /// モジュール一覧
        /// </summary>
        public ObservablePropertyChangedCollection<ModulesListItem> Modules { get; private set; } = new ObservablePropertyChangedCollection<ModulesListItem>();
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
                bool chked = 0 < DBConnection.CommonDB.ExecQuery($"SELECT * FROM SelectModuleCheckStateModuleTypes WHERE ID = '{dr["ModuleTypeID"]}'", (_, __) => { });
                items.Add(new ModulesListItem((string)dr["ModuleTypeID"], (string)dr["Name"], chked));
            }

            DBConnection.X4DB.ExecQuery(@"
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
                bool isChecked = 0 < DBConnection.CommonDB.ExecQuery($"SELECT * FROM SelectModuleCheckStateModuleOwners WHERE ID = '{dr["FactionID"]}'", (_, __) => { });

                var faction = Faction.Get((string)dr["FactionID"]);
                if (faction != null) items.Add(new FactionsListItem(faction, isChecked));
            }

            DBConnection.X4DB.ExecQuery(@"
SELECT
    FactionID,
    Name
FROM
    Faction
WHERE
    FactionID IN (SELECT FactionID FROM ModuleOwner)
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
    DISTINCT Module.ModuleID,
	Module.Name
FROM
    Module,
	ModuleOwner
WHERE
	Module.ModuleID = ModuleOwner.ModuleID AND
    Module.NoBlueprint = 0 AND
    Module.ModuleTypeID   IN ({string.Join(", ", ModuleTypes.Where(x => x.IsChecked).Select(x => $"'{x.ID}'"))}) AND
	ModuleOwner.FactionID IN ({string.Join(", ", ModuleOwners.Where(x => x.IsChecked).Select(x => $"'{x.Faction.FactionID}'"))})";

            var list = new List<ModulesListItem>();
            DBConnection.X4DB.ExecQuery(query, SetModules, list);
            Modules.Reset(list);
        }



        /// <summary>
        /// モジュール一覧用ListViewを初期化する
        /// </summary>
        /// <param name="SQLiteDataReader">クエリ結果</param>
        /// <param name="args">可変長引数</param>
        private void SetModules(SQLiteDataReader dr, object[] args)
        {
            var list = (List<ModulesListItem>)args[0];
            list.Add(new ModulesListItem((string)dr["ModuleID"], (string)dr["Name"], false));
        }


        /// <summary>
        /// 選択中のモジュール一覧をコレクションに追加する
        /// </summary>
        public void AddSelectedModuleToItemCollection()
        {
            // 選択されているアイテムを追加
            var items = Modules.Where(x => x.IsChecked)
                               .Select(x => DB.X4DB.Module.Get(x.ID))
                               .Where(x => x != null)
                               .Select(x => x!)
                               .Select(x => new ModulesGridItem(x) { EditStatus = EditStatus.Edited });

            ItemCollection.AddRange(items);
        }

        /// <summary>
        /// チェック状態を保存する
        /// </summary>
        public void SaveCheckState()
        {
            // 前回値クリア
            DBConnection.CommonDB.ExecQuery("DELETE FROM SelectModuleCheckStateModuleTypes", null);
            DBConnection.CommonDB.ExecQuery("DELETE FROM SelectModuleCheckStateModuleOwners", null);

            // トランザクション開始
            DBConnection.CommonDB.BeginTransaction();

            // モジュール種別のチェック状態保存
            foreach (var id in ModuleTypes.Where(x => x.IsChecked).Select(x => x.ID))
            {
                DBConnection.CommonDB.ExecQuery($"INSERT INTO SelectModuleCheckStateModuleTypes(ID) VALUES ('{id}')", null);
            }

            // 派閥一覧のチェック状態保存
            foreach (var id in ModuleOwners.Where(x => x.IsChecked).Select(x => x.Faction.FactionID))
            {
                DBConnection.CommonDB.ExecQuery($"INSERT INTO SelectModuleCheckStateModuleOwners(ID) VALUES ('{id}')", null);
            }

            // コミット
            DBConnection.CommonDB.Commit();
        }
    }
}
