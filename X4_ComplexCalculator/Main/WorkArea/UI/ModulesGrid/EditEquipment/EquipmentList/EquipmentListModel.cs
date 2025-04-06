using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList;


/// <summary>
/// 装備リストのModel
/// </summary>
sealed partial class EquipmentListModel : ObservableRecipientEx
{
    #region メンバ
    /// <summary>
    /// 装備管理用(作業用)
    /// </summary>
    private readonly EquippableWareEquipmentManager _tempManager;


    /// <summary>
    /// 装備種別
    /// </summary>
    private readonly IEquipmentType _equipmentType;
    #endregion


    #region プロパティ
    /// <summary>
    /// タイトル文字列
    /// </summary>
    public string Title => _equipmentType.Name;


    /// <summary>
    /// 現在のサイズ
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial IX4Size SelectedSize { get; set; }


    /// <summary>
    /// 装備可能な装備一覧
    /// </summary>
    public ObservableRangeCollection<EquipmentListItem> Equippable { get; } = [];


    /// <summary>
    /// 装備済みの装備一覧
    /// </summary>
    public ObservableRangeCollection<EquipmentListItem> Equipped { get; } = [];


    /// <summary>
    /// 装備可能な個数
    /// </summary>
    public int MaxAmount => _tempManager.GetMaxEquippableCount(_equipmentType, SelectedSize);


    /// <summary>
    /// 装備済みの個数
    /// </summary>
    public int EquippedCount => _tempManager.AllEquipments.Count(x => x.EquipmentType.Equals(_equipmentType) && x.EquipmentTags.Contains(SelectedSize.SizeID));


    /// <summary>
    /// 選択中のプリセット
    /// </summary>
    [ObservableProperty]
    public partial PresetComboboxItem? SelectedPreset { get; set; }


    /// <summary>
    /// 保存済みでないか
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial bool Unsaved { get; set; }


    /// <summary>
    /// 派閥一覧
    /// </summary>
    public IReadOnlyCollection<FactionsListItem> Factions { get; }
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messenger">メッセージ通知用</param>
    /// <param name="manager"></param>
    /// <param name="equipmentTypeID"></param>
    /// <param name="factions"></param>
    public EquipmentListModel(
        IMessenger messenger,
        EquippableWareEquipmentManager manager,
        IEquipmentType equipmentType,
        IX4Size size,
        IReadOnlyCollection<FactionsListItem> factions
    ) : base(messenger, false)
    {
        _equipmentType = equipmentType;
        _tempManager   = new EquippableWareEquipmentManager(manager);
        Factions       = factions;
        SelectedSize   = size;

        // 装備可能な装備一覧を作成
        {
            var equipments = X4Database.Instance.Ware.GetAll<IEquipment>()
                .Where(x => x.EquipmentType.Equals(equipmentType) && !x.EquipmentTags.Contains("unhittable"))
                .Select(x => new EquipmentListItem(x));
            Equippable.AddRange(equipments);
        }

        // 装備済みの装備一覧を作成
        {
            var equipments = _tempManager.AllEquipments
                .Where(x => x.EquipmentType.Equals(equipmentType))
                .Select(x => new EquipmentListItem(x));
            Equipped.AddRange(equipments);
        }

        IsActive = true;
    }


    /// <summary>
    /// 選択された装備を追加
    /// </summary>
    /// <param name="addMaximum">最大まで追加するか</param>
    public void AddSelectedEquipments(bool addMaximum)
    {
        if (SelectedSize is null) return;

        var oldEquippedCount = EquippedCount;

        var addItems = Equippable.Where(x => x.IsSelected);
        var addRange = _tempManager.GetEquippableCount(_equipmentType, SelectedSize);

        var added = false;

        if (0 < addRange)
        {
            do
            {
                // 追加可能な分だけ追加する
                var addTarget = addItems.Take(addRange).Select(x => x.Equipment).ToArray();
                Equipped.AddRange(addTarget.Select(x => new EquipmentListItem(x)));
                _tempManager.AddRange(addTarget.Select(x => x));

                // 再計算
                addRange = _tempManager.GetEquippableCount(_equipmentType, SelectedSize);

                added = true;
            }
            while (0 < addRange && addMaximum);
        }

        if (added)
        {
            Unsaved = true;
            OnPropertyChanged(nameof(EquippedCount));
            Broadcast(oldEquippedCount, EquippedCount, nameof(EquippedCount));
        }
    }



    /// <summary>
    /// 装備を削除
    /// </summary>
    /// <returns>装備が削除されたか</returns>
    public void RemoveSelectedEquipments()
    {
        if (SelectedSize is null)
        {
            throw new InvalidOperationException();
        }

        if (Equipped.Any(x => x.IsSelected))
        {
            var oldEquippedCount = EquippedCount;

            _tempManager.RemoveRange(Equipped.Where(x => x.IsSelected).Select(x => x.Equipment));
            Equipped.RemoveAll(x => x.IsSelected);

            OnPropertyChanged(nameof(EquippedCount));
            Broadcast(oldEquippedCount, EquippedCount, nameof(EquippedCount));

            Unsaved = true;
        }
    }


    /// <summary>
    /// 選択サイズ変更時
    /// </summary>
    partial void OnSelectedSizeChanged(IX4Size oldValue, IX4Size newValue)
    {
        if (oldValue is null) return;

        var oldMaxAmount = _tempManager.GetMaxEquippableCount(_equipmentType, oldValue);
        var oldEquippedCount = _tempManager.AllEquipments.Count(x => x.EquipmentType.Equals(_equipmentType) && x.EquipmentTags.Contains(oldValue.SizeID));

        OnPropertyChanged(nameof(MaxAmount));
        Broadcast(oldMaxAmount, MaxAmount, nameof(MaxAmount));

        OnPropertyChanged(nameof(EquippedCount));
        Broadcast(oldEquippedCount, EquippedCount, nameof(EquippedCount));
    }



    /// <summary>
    /// プリセット変更時
    /// </summary>
    partial void OnSelectedPresetChanged(PresetComboboxItem? value)
    {
        if (value is null)
        {
            return;
        }

        const string QUERY = @"
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
            ModuleID = _tempManager.Ware.ID,
            PresetID = value.ID,
            EquipmentType = _equipmentType.EquipmentTypeID
        };

        var equipments = SettingDatabase.Instance.Query<string>(QUERY, param)
            .Select(x => X4Database.Instance.Ware.Get<IEquipment>(x))
            .Select(x => new EquipmentListItem(x));

        var oldEquippedCount = EquippedCount;

        Equipped.Reset(equipments);
        _tempManager.ResetEquipment(equipments.Select(x => x.Equipment));

        OnPropertyChanged(nameof(EquippedCount));
        Broadcast(oldEquippedCount, EquippedCount, nameof(EquippedCount));

        Unsaved = true;
    }
}
