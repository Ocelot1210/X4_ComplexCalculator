using System.Collections.Generic;
using System.Data.SQLite;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using System.Linq;

namespace X4_ComplexCalculator.Main.ModulesGrid.EditEquipment
{
    /// <summary>
    /// 装備編集画面のModel
    /// </summary>
    class EditEquipmentModel
    {
        #region プロパティ
        /// <summary>
        /// 装備サイズ一覧
        /// </summary>
        public SmartCollection<Size> EquipmentSizes { get; private set; } = new SmartCollection<Size>();


        /// <summary>
        /// 種族一覧
        /// </summary>
        public MemberChangeDetectCollection<FactionsListItem> Factions { get; } = new MemberChangeDetectCollection<FactionsListItem>();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">編集対象モジュール</param>
        public EditEquipmentModel(Module module)
        {
            // 初期化
            InitEquipmentSizes(module.ModuleID);
            UpdateFactions();
        }


        /// <summary>
        /// 装備サイズコンボボックスの内容を初期化
        /// </summary>
        /// <param name="moduleID"></param>
        private void InitEquipmentSizes(string moduleID)
        {
            void AddItem(SQLiteDataReader dr, object[] args)
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
            void AddItem(SQLiteDataReader dr, object[] args)
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
    }
}
