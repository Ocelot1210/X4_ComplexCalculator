using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.ModulesGrid.EditEquipment.EquipmentList
{
    /// <summary>
    /// タレットの装備を管理
    /// </summary>
    class TurretEquipmentListModel : EquipmentListModelBase
    {
        #region メンバ
        /// <summary>
        ///  選択中のプリセット
        /// </summary>
        private PresetComboboxItem _SelectedPreset;
        #endregion


        #region プロパティ
        /// <summary>
        /// 選択中のプリセット
        /// </summary>
        public override PresetComboboxItem SelectedPreset
        {
            protected get => _SelectedPreset;
            set
            {
                _SelectedPreset = value;
                if (_SelectedPreset != null)
                {
                    OnSelectedPresetChanged();
                }
            }
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module"></param>
        /// <param name="factions"></param>
        public TurretEquipmentListModel(Module module, ObservablePropertyChangedCollection<FactionsListItem> factions) : base(module, factions)
        {
            foreach (Size size in Module.Equipment.Turret.Sizes)
            {
                _Equipments.Add(size, new ObservableRangeCollection<Equipment>());
                _Equipped.Add(size, new ObservableRangeCollection<Equipment>());
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
        public override void OnPresetsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

            if (e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Add)
            {

                foreach (PresetComboboxItem item in e.NewItems)
                {
                    foreach (var equipment in Equipped.Values.SelectMany((x) => x))
                    {
                        var query = $"INSERT INTO ModulePresetsEquipment(ModuleID, PresetID, EquipmentID, EquipmentType) VALUES('{Module.ModuleID}', {item.ID}, '{equipment.EquipmentID}', '{equipment.EquipmentType.EquipmentTypeID}')";
                        DBConnection.CommonDB.ExecQuery(query);
                    }
                }
            }
        }


        /// <summary>
        /// 装備一覧を更新
        /// </summary>
        public override void UpdateEquipments(object sender, PropertyChangedEventArgs e)
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

            DBConnection.X4DB.ExecQuery(query, (SQLiteDataReader dr, object[] args) => { items.Add(new Equipment((string)dr["EquipmentID"])); });

            Equipments[SelectedSize].Reset(items);
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


        /// <summary>
        /// 選択中のプリセット変更時
        /// </summary>
        private void OnSelectedPresetChanged()
        {
            var query = @$"
SELECT
    EquipmentID

FROM
    ModulePresetsEquipment

WHERE
    ModuleID      = '{Module.ModuleID}' AND
    PresetID      = {SelectedPreset.ID} AND
    EquipmentType = 'turrets'";

            var equipments = new List<Equipment>(_MaxAmount.Values.Count());

            DBConnection.CommonDB.ExecQuery(query, (dr, args) =>
            {
                equipments.Add(new Equipment((string)dr["EquipmentID"]));
            });

            foreach (var size in Module.Equipment.Turret.Sizes)
            {
                _Equipped[size].Clear();
                _Equipped[size].AddRange(equipments.Where(x => x.Size.Equals(size)));
            }
        }
    }
}
