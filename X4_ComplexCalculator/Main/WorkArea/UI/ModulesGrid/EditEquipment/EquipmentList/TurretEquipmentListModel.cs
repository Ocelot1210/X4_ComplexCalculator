using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList
{
    /// <summary>
    /// タレット一覧用Model
    /// </summary>
    class TurretEquipmentListModel : EquipmentListModelBase
    {
        #region メンバ
        /// <summary>
        ///  選択中のプリセット
        /// </summary>
        private PresetComboboxItem? _SelectedPreset;
        #endregion


        #region プロパティ
        /// <summary>
        /// 選択中のプリセット
        /// </summary>
        public override PresetComboboxItem? SelectedPreset
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
        public TurretEquipmentListModel(ModulesGridItem module, ObservablePropertyChangedCollection<FactionsListItem> factions) : base(module, factions)
        {
            foreach (Size size in Module.ModuleEquipment.Turret.Sizes)
            {
                _Equipments.Add(size, new ObservableRangeCollection<EquipmentListItem>());
                _Equipped.Add(size, new ObservableRangeCollection<EquipmentListItem>());
                _MaxAmount.Add(size, Module.ModuleEquipment.Turret.MaxAmount[size]);

                // 前回値復元
                var equipments = Module.ModuleEquipment.Turret.GetEquipment(size).Take(_MaxAmount[size]).Select(x => new EquipmentListItem(x));
                _Equipped[size].AddRange(equipments);
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
                foreach (PresetComboboxItem? item in e.OldItems)
                {
                    if (item == null)
                    {
                        throw new InvalidOperationException();
                    }

                    var query = @$"
DELETE FROM
    ModulePresetsEquipment

WHERE
    ModuleID = '{Module.Module.ModuleID}' AND
    PresetID = {item.ID} AND
    EquipmentType = 'turrets'";
                    DBConnection.CommonDB.ExecQuery(query);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Add)
            {

                foreach (PresetComboboxItem? item in e.NewItems)
                {
                    if (item == null)
                    {
                        throw new InvalidOperationException();

                    }
                    foreach (var equipment in Equipped.Values.SelectMany((x) => x))
                    {
                        var query = @$"
INSERT INTO
    ModulePresetsEquipment(ModuleID, PresetID, EquipmentID, EquipmentType)

VALUES(
    '{Module.Module.ModuleID}',
    {item.ID},
    '{equipment.Equipment.EquipmentID}',
    '{equipment.Equipment.EquipmentType.EquipmentTypeID}'
)";
                        DBConnection.CommonDB.ExecQuery(query);
                    }
                }
            }
        }


        /// <summary>
        /// 装備一覧を更新メイン
        /// </summary>
        protected override void UpdateEquipmentsMain()
        {
            if (SelectedSize == null)
            {
                return;
            }

            var items = new List<EquipmentListItem>();

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
    Equipment.EquipmentID IN (SELECT EquipmentResource.EquipmentID FROM EquipmentResource) AND
    EquipmentOwner.FactionID IN ({selectedFactions})
";

            DBConnection.X4DB.ExecQuery(query, (dr, args) => { items.Add(new EquipmentListItem((string)dr["EquipmentID"])); });

            Equipments[SelectedSize].Reset(items);
        }


        /// <summary>
        /// 装備を保存
        /// </summary>
        public override void SaveEquipment()
        {
            foreach (var size in _Equipments.Keys)
            {
                Module.ModuleEquipment.Turret.ResetEquipment(size, Equipped[size].Select(x => x.Equipment).ToArray());
            }
        }


        /// <summary>
        /// 選択中のプリセット変更時
        /// </summary>
        private void OnSelectedPresetChanged()
        {
            if (SelectedPreset == null)
            {
                throw new InvalidOperationException();
            }

            var query = @$"
SELECT
    EquipmentID

FROM
    ModulePresetsEquipment

WHERE
    ModuleID      = '{Module.Module.ModuleID}' AND
    PresetID      = {SelectedPreset.ID} AND
    EquipmentType = 'turrets'";

            var equipments = new List<EquipmentListItem>(_MaxAmount.Values.Count());

            DBConnection.CommonDB.ExecQuery(query, (dr, args) =>
            {
                equipments.Add(new EquipmentListItem((string)dr["EquipmentID"]));
            });

            foreach (var size in Module.ModuleEquipment.Turret.Sizes)
            {
                _Equipped[size].Clear();
                _Equipped[size].AddRange(equipments.Where(x => x.Equipment.Size.Equals(size)));
            }
        }
    }
}
