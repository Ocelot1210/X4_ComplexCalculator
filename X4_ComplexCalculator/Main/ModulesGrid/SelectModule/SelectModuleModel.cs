using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main.ModulesGrid.SelectModule
{
    class SelectModuleModel
    {
        #region メンバ変数
        /// <summary>
        /// モジュール追加先
        /// </summary>
        private SmartCollection<ModulesGridItem> ItemCollection;
        #endregion

        #region プロパティ
        /// <summary>
        /// モジュール種別
        /// </summary>
        public MemberChangeDetectCollection<ModulesListItem> ModuleTypes { get; private set; } = new MemberChangeDetectCollection<ModulesListItem>();


        /// <summary>
        /// モジュール所有派閥
        /// </summary>
        public MemberChangeDetectCollection<ModulesListItem> ModuleOwners { get; private set; } = new MemberChangeDetectCollection<ModulesListItem>();


        /// <summary>
        /// モジュール一覧
        /// </summary>
        public MemberChangeDetectCollection<ModulesListItem> Modules { get; private set; } = new MemberChangeDetectCollection<ModulesListItem>();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="itemCollection">選択結果格納先</param>
        public SelectModuleModel(SmartCollection<ModulesGridItem> itemCollection)
        {
            ItemCollection = itemCollection;

            Task.WaitAll(InitModuleTypes(), InitModuleOwners(), UpdateModules(null, null));

            ModuleOwners.OnCollectionOrPropertyChanged += UpdateModules;
            ModuleTypes.OnCollectionOrPropertyChanged += UpdateModules;
        }


        /// <summary>
        /// モジュール種別一覧を初期化する
        /// </summary>
        private async Task InitModuleTypes()
        {
            var items = new List<ModulesListItem>();

            void init(SQLiteDataReader dr, object[] args)
            {
                bool chked = 0 < DBConnection.CommonDB.ExecQuery($"SELECT * FROM SelectModuleCheckStateModuleTypes WHERE ID = '{dr["ModuleTypeID"]}'", (SQLiteDataReader drDummy, object[] argsDummy) => { });
                items.Add(new ModulesListItem(dr["ModuleTypeID"].ToString(), dr["Name"].ToString(), chked));
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

            await Task.CompletedTask;
        }


        /// <summary>
        /// 派閥一覧を初期化する
        /// </summary>
        private async Task InitModuleOwners()
        {
            var items = new List<ModulesListItem>();

            void init(SQLiteDataReader dr, object[] args)
            {
                bool chked = 0 < DBConnection.CommonDB.ExecQuery($"SELECT * FROM SelectModuleCheckStateModuleOwners WHERE ID = '{dr["FactionID"]}'", (SQLiteDataReader drDummy, object[] argsDummy) => { });

                items.Add(new ModulesListItem(dr["FactionID"].ToString(), dr["Name"].ToString(), chked));
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

            await Task.CompletedTask;
        }



        /// <summary>
        /// モジュール一覧を更新する
        /// </summary>
        public async Task UpdateModules(object sender, NotifyCollectionChangedEventArgs e)
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
    Module.ModuleTypeID   IN ({string.Join(", ", ModuleTypes.Where(x => x.Checked).Select(x => $"'{x.ID}'"))}) AND
	ModuleOwner.FactionID IN ({string.Join(", ", ModuleOwners.Where(x => x.Checked).Select(x => $"'{x.ID}'"))})";

            var list = new List<ModulesListItem>();
            DBConnection.X4DB.ExecQuery(query, SetModules, list);
            Modules.Reset(list);

            await Task.CompletedTask;
        }


        /// <summary>
        /// モジュール一覧用ListViewを初期化する
        /// </summary>
        /// <param name="SQLiteDataReader">クエリ結果</param>
        /// <param name="args">可変長引数</param>
        private void SetModules(SQLiteDataReader dr, object[] args)
        {
            var list = (List<ModulesListItem>)args[0];
            list.Add(new ModulesListItem(dr["ModuleID"].ToString(), dr["Name"].ToString(), false));
        }


        /// <summary>
        /// 選択中のモジュール一覧をコレクションに追加する
        /// </summary>
        public void AddSelectedModuleToItemCollection()
        {
            // 選択されているアイテムを追加
            var items = Modules.Where(x => x.Checked).Select(x => new ModulesGridItem(x.ID));
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
            foreach (var id in ModuleTypes.Where(x => x.Checked).Select(x => x.ID))
            {
                DBConnection.CommonDB.ExecQuery($"INSERT INTO SelectModuleCheckStateModuleTypes(ID) VALUES ('{id}')", null);
            }

            // 派閥一覧のチェック状態保存
            foreach (var id in ModuleOwners.Where(x => x.Checked).Select(x => x.ID))
            {
                DBConnection.CommonDB.ExecQuery($"INSERT INTO SelectModuleCheckStateModuleOwners(ID) VALUES ('{id}')", null);
            }

            // コミット
            DBConnection.CommonDB.Commit();
        }
    }
}
