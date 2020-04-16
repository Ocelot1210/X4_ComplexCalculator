using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.ModulesGrid.EditEquipment.EquipmentList
{
    /// <summary>
    /// タレットの装備を管理
    /// </summary>
    class TurretEquipmentListModel : EquipmentListModelBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module"></param>
        /// <param name="factions"></param>
        public TurretEquipmentListModel(Module module, EditEquipmentViewModel viewModel) : base(module, viewModel)
        {
            viewModel.Presets.CollectionChanged += OnPresetsCollectionChanged;

            foreach (Size size in Module.Equipment.Turret.Sizes)
            {
                _Equipments.Add(size, new SmartCollection<Equipment>());
                _Equipped.Add(size, new SmartCollection<Equipment>());
                _MaxAmount.Add(size, Module.Equipment.Turret.MaxAmount[size]);

                // 前回値復元
                _Equipped[size].AddRange(Module.Equipment.Turret.GetEquipment(size).Take(_MaxAmount[size]));
            }
        }

        /// <summary>
        /// プリセット一覧変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnPresetsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PresetComboboxItem item in e.OldItems)
                {
                    var query = @$"
DELETE FROM
    ModulePresetsEquipment

WHERE
    ModuleID = '{Module.ModuleID}' AND
    PresetID = {item.ID} AND
    EquipmentType = 'turrets'";
                    DBConnection.CommonDB.ExecQuery(query);
                }
            }

            base.OnPresetsCollectionChanged(sender, e);
        }


        /// <summary>
        /// 装備一覧を更新
        /// </summary>
        protected override async Task UpdateEquipments(object sender, PropertyChangedEventArgs e)
        {
            if (SelectedSize == null)
            {
                return;
            }

            var items = new List<Equipment>();

            var selectedFactions = string.Join(", ", SelectedFactions.Select(x => $"'{x.Faction.FactionID}'"));

            var query = $@"
SELECT
	DISTINCT Equipment.EquipmentID AS EquipmentID
FROM
	Equipment,
	EquipmentOwner
WHERE
	EquipmentTypeID = 'turrets' AND
	SizeID = '{SelectedSize.SizeID}' AND
	Equipment.EquipmentID = EquipmentOwner.EquipmentID AND
    EquipmentOwner.FactionID IN ({selectedFactions})
";

            DBConnection.X4DB.ExecQuery(query, (SQLiteDataReader dr, object[] args) => { items.Add(new Equipment(dr["EquipmentID"].ToString())); });

            Equipments[SelectedSize].Reset(items);
            await Task.CompletedTask;
        }


        /// <summary>
        /// 装備を保存
        /// </summary>
        public override void SaveEquipment()
        {
            foreach(var size in _Equipments.Keys)
            {
                Module.Equipment.Turret.ResetEquipment(size, Equipped[size]);
            }
        }
    }
}
