﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList
{
    /// <summary>
    /// シールド一覧用Model
    /// </summary>
    class ShieldEquipmentListModel : EquipmentListModelBase
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
                if (_SelectedPreset is not null)
                {
                    OnSelectedPresetChanged();
                }
            }
        }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">編集対象モジュール</param>
        /// <param name="factions">種族一覧</param>
        public ShieldEquipmentListModel(ModulesGridItem module, ObservablePropertyChangedCollection<FactionsListItem> factions) : base(module, factions)
        {
            //foreach (X4Size size in Module.Equipments.Shield.Sizes)
            //{
            //    _Equipments.Add(size, new ObservableRangeCollection<EquipmentListItem>());
            //    _Equipped.Add(size, new ObservableRangeCollection<EquipmentListItem>());
            //    _MaxAmount.Add(size, Module.Equipments.Shield.MaxAmount[size]);

            //    // 前回値復元
            //    var equipments = Module.Equipments.Shield.GetEquipment(size).Take(_MaxAmount[size]).Select(x => new EquipmentListItem(x));
            //    _Equipped[size].AddRange(equipments);
            //}
        }


        /// <summary>
        /// プリセット一覧変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnPresetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PresetComboboxItem? item in e.OldItems)
                {
                    if (item is null)
                    {
                        throw new InvalidOperationException();
                    }

                    var query = @$"
DELETE FROM
    ModulePresetsEquipment

WHERE
    ModuleID = '{Module.Module.ID}' AND
    PresetID = {item.ID} AND
    EquipmentType = 'shields'";
                    SettingDatabase.Instance.ExecQuery(query);
                }
            }


            if (e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Add)
            {

                foreach (PresetComboboxItem? item in e.NewItems)
                {
                    if (item is null)
                    {
                        throw new InvalidOperationException();
                    }

                    foreach (var equipment in Equipped.Values.SelectMany((x) => x))
                    {
                        var query = @$"
INSERT INTO
    ModulePresetsEquipment(ModuleID, PresetID, EquipmentID, EquipmentType)

VALUES(
    '{Module.Module.ID}',
    {item.ID},
    '{equipment.Equipment.ID}',
    '{equipment.Equipment.EquipmentType.EquipmentTypeID}'
)";
                        SettingDatabase.Instance.ExecQuery(query);
                    }
                }
            }
        }


        /// <summary>
        /// 装備一覧を更新メイン
        /// </summary>
        protected override void UpdateEquipmentsMain()
        {
            if (SelectedSize is null)
            {
                return;
            }

            var items = new List<EquipmentListItem>();

            var selectedFactions = string.Join(", ", SelectedFactions.Select(x => $"'{x.Faction.FactionID}'"));

            var query = $@"
SELECT
	DISTINCT Equipment.EquipmentID
FROM
	Equipment,
	EquipmentOwner
WHERE
	EquipmentTypeID = 'shields' AND
	SizeID = '{SelectedSize.SizeID}' AND
	Equipment.EquipmentID = EquipmentOwner.EquipmentID AND
    Equipment.EquipmentID IN (SELECT EquipmentResource.EquipmentID FROM EquipmentResource) AND
    EquipmentOwner.FactionID IN ({selectedFactions})";

            X4Database.Instance.ExecQuery(query, (dr, _) =>
            {
                var eqp = Ware.TryGet<Equipment>((string)dr["EquipmentID"]);
                if (eqp is not null)
                {
                    items.Add(new EquipmentListItem(eqp));
                }
            });

            Equipments[SelectedSize].Reset(items);
        }


        /// <summary>
        /// 装備を保存
        /// </summary>
        public override void SaveEquipment()
        {
            //foreach (var size in _Equipments.Keys)
            //{
            //    Module.Equipments.Shield.ResetEquipment(size, Equipped[size].Select(x => x.Equipment).ToArray());
            //}
        }


        /// <summary>
        /// 選択中のプリセット変更時
        /// </summary>
        private void OnSelectedPresetChanged()
        {
//            if (SelectedPreset is null)
//            {
//                throw new InvalidOperationException();
//            }

//            var query = @$"
//SELECT
//    EquipmentID

//FROM
//    ModulePresetsEquipment

//WHERE
//    ModuleID      = '{Module.Module.ID}' AND
//    PresetID      = {SelectedPreset.ID} AND
//    EquipmentType = 'shields'";

//            var equipments = new List<EquipmentListItem>(_MaxAmount.Values.Count);

//            SettingDatabase.Instance.ExecQuery(query, (dr, _) =>
//            {
//                var eqp = Ware.TryGet<Equipment>((string)dr["EquipmentID"]);
//                if (eqp is not null)
//                {
//                    equipments.Add(new EquipmentListItem(eqp));
//                }
//            });

//            foreach (var size in Module.Equipments.Shield.Sizes)
//            {
//                _Equipped[size].Clear();
//                _Equipped[size].AddRange(equipments.Where(x => x.Equipment.Size.Equals(size)));
//            }
        }
    }
}
