using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Data;
using System.Linq;
using System.Windows.Input;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Entity;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList
{
    /// <summary>
    /// 装備リストのModel
    /// </summary>
    class EquipmentListModel : BindableBase
    {
        #region メンバ
        /// <summary>
        /// 装備管理用(作業用)
        /// </summary>
        private readonly WareEquipmentManager _TempManager;


        /// <summary>
        /// 装備種別
        /// </summary>
        private readonly EquipmentType _EquipmentType;
        #endregion


        #region プロパティ
        /// <summary>
        /// タイトル文字列
        /// </summary>
        public string Title => _EquipmentType.Name;


        /// <summary>
        /// 現在のサイズ
        /// </summary>
        public ReactiveProperty<X4Size> SelectedSize { get; }


        /// <summary>
        /// 装備可能な装備一覧
        /// </summary>
        public ObservablePropertyChangedCollection<EquipmentListItem> Equippable { get; } = new();


        /// <summary>
        /// 装備済みの装備一覧
        /// </summary>
        public ObservablePropertyChangedCollection<EquipmentListItem> Equipped { get; } = new();


        /// <summary>
        /// 装備可能な個数
        /// </summary>
        public int MaxAmount => _TempManager.GetMaxEquippableCount(_EquipmentType, SelectedSize.Value);


        /// <summary>
        /// 装備済みの個数
        /// </summary>
        public int EquippedCount => _TempManager.AllEquipments.Count(x => x.EquipmentType.Equals(_EquipmentType) && x.EquipmentTags.Contains(SelectedSize.Value.SizeID));


        /// <summary>
        /// 選択中のプリセット
        /// </summary>
        public ReactiveProperty<PresetComboboxItem?> SelectedPreset { get; }


        /// <summary>
        /// 保存済みでないか
        /// </summary>
        public ReactiveProperty<bool> Unsaved { get; } = new(false);
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="equipmentTypeID"></param>
        /// <param name="factions"></param>
        public EquipmentListModel(
            WareEquipmentManager manager,
            EquipmentType equipmentType,
            X4Size size
        )
        {
            _EquipmentType = equipmentType;
            _TempManager = new WareEquipmentManager(manager);

            SelectedPreset = new ();
            SelectedPreset.Subscribe(x => PresetChanged());

            SelectedSize = new(size);
            SelectedSize.Subscribe(_ => 
            {
                RaisePropertyChanged(nameof(MaxAmount));
                RaisePropertyChanged(nameof(EquippedCount));
            });


            // 装備可能な装備一覧を作成
            {
                var equipments = Ware.GetAll<Equipment>()
                    .Where(x => x.EquipmentType.Equals(equipmentType) && !x.EquipmentTags.Contains("unhittable"))
                    .Select(x => new EquipmentListItem(x));
                Equippable.AddRange(equipments);
            }

            // 装備済みの装備一覧を作成
            {
                var equipments = _TempManager.AllEquipments
                    .Where(x => x.EquipmentType.Equals(equipmentType))
                    .Select(x => new EquipmentListItem(x));
                Equipped.AddRange(equipments);
            }
        }


        /// <summary>
        /// 選択された装備を追加
        /// </summary>
        /// <returns>装備が追加されたか</returns>
        public bool AddSelectedEquipments()
        {
            if (SelectedSize is null)
            {
                return false;
            }

            var addItems = Equippable.Where(x => x.IsSelected);
            var addRange = _TempManager.GetEquippableCount(_EquipmentType, SelectedSize.Value);

            var added = false;

            // 左Shiftキー押下時なら選択アイテムを全追加
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                while (0 < addRange)
                {
                    // 追加可能な分だけ追加する
                    var addTarget = addItems.Take(addRange).Select(x => x.Equipment).ToArray();
                    Equipped.AddRange(addTarget.Select(x => new EquipmentListItem(x)));
                    _TempManager.AddRange(addTarget.Select(x => x));

                    // 再計算
                    addRange = _TempManager.GetEquippableCount(_EquipmentType, SelectedSize.Value);

                    added = true;
                }
            }
            else
            {
                if (0 < addRange)
                {
                    // 追加可能な分だけ追加する
                    var addTarget = addItems.Take(addRange).Select(x => x.Equipment).ToArray();
                    Equipped.AddRange(addTarget.Select(x => new EquipmentListItem(x)));
                    _TempManager.AddRange(addTarget.Select(x => x));

                    added = true;
                }
            }

            if (added)
            {
                Unsaved.Value = true;
                RaisePropertyChanged(nameof(EquippedCount));
            }

            return added;
        }



        /// <summary>
        /// 装備を削除
        /// </summary>
        /// <returns>装備が削除されたか</returns>
        public bool RemoveSelectedEquipments()
        {
            if (SelectedSize is null)
            {
                throw new InvalidOperationException();
            }

            if (Equipped.Any(x => x.IsSelected))
            {
                _TempManager.RemoveRange(Equipped.Where(x => x.IsSelected).Select(x => x.Equipment));
                Equipped.RemoveAll(x => x.IsSelected);
                RaisePropertyChanged(nameof(EquippedCount));
                Unsaved.Value = true;
            }

            return false;
        }


        /// <summary>
        /// プリセット変更時
        /// </summary>
        private void PresetChanged()
        {
            if (SelectedPreset.Value is null)
            {
                return;
            }

            const string query = @"
SELECT
    EquipmentID
FROM
    ModulePresetsEquipment
WHERE
    ModuleID = :ModuleID AND
    PresetID = :PresetID AND
    EquipmentType = :EquipmentType";

            var param = new 
            {
                ModuleID = _TempManager.Ware.ID,
                PresetID = SelectedPreset.Value.ID,
                EquipmentType = _EquipmentType.EquipmentTypeID
            };

            var equipments = SettingDatabase.Instance.Query<string>(query, param)
                .Select(x => Ware.Get<Equipment>(x))
                .Select(x => new EquipmentListItem(x));

            Equipped.Reset(equipments);
            _TempManager.ResetEquipment(equipments.Select(x => x.Equipment));


            RaisePropertyChanged(nameof(EquippedCount));
            Unsaved.Value = true;
        }
    }
}
