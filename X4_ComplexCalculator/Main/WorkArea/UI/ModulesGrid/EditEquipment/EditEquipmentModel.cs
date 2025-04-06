using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Linq;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.Common.Dialogs.SelectStringDialog;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

/// <summary>
/// 装備編集画面のModel
/// </summary>
sealed partial class EditEquipmentModel : ObservableRecipientEx, IDisposable
{
    #region メンバ
    /// <summary>
    /// 編集対象の装備管理
    /// </summary>
    private readonly EquippableWareEquipmentManager _manager;


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox;


    /// <summary>
    /// プリセット削除中か
    /// </summary>
    private bool _removingPreset = false;
    #endregion


    #region プロパティ
    /// <summary>
    /// 装備サイズ一覧
    /// </summary>
    public ObservableRangeCollection<IX4Size> EquipmentSizes { get; } = [];


    /// <summary>
    /// 派閥一覧
    /// </summary>
    public ObservableRangeCollection<FactionsListItem> Factions { get; } = [];


    /// <summary>
    /// プリセット一覧
    /// </summary>
    public ObservableRangeCollection<PresetComboboxItem> Presets { get; } = [];


    /// <summary>
    /// 選択中のプリセット
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial PresetComboboxItem? SelectedPreset { get; set; }


    /// <summary>
    /// 選択中のサイズ
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial IX4Size SelectedSize { get; set; }


    /// <summary>
    /// タブアイテム一覧
    /// </summary>
    public ObservableRangeCollection<EquipmentListViewModel> EquipmentListViewModels { get; } = [];
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messenger">メッセージ通知用</param>
    /// <param name="ware">編集対象ウェア</param>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    public EditEquipmentModel(IMessenger messenger, EquippableWareEquipmentManager equipmentManager, ILocalizedMessageBox localizedMessageBox) : base(messenger, false)
    {
        // 初期化
        _manager = equipmentManager;
        _localizedMessageBox = localizedMessageBox;

        InitEquipmentSizes();
        UpdateFactions();
        InitPreset();

        SelectedSize = EquipmentSizes.First();

        {
            string[] types = ["turrets", "shields"];

            var viewModels = types
                .Select(x => X4Database.Instance.EquipmentType.Get(x))
                .Select(x => new EquipmentListModel(Messenger, equipmentManager, x, SelectedSize, Factions))
                .Select(x => new EquipmentListViewModel(Messenger, x));

            EquipmentListViewModels.AddRange(viewModels);
        }

        IsActive = true;
    }


    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var vm in EquipmentListViewModels)
        {
            vm.Dispose();
        }
    }


    /// <summary>
    /// 装備サイズコンボボックスの内容を初期化
    /// </summary>
    private void InitEquipmentSizes()
    {
        var sizes = _manager.Ware.Equipments.Values
            .SelectMany(x => x.Tags)
            .Distinct()
            .Select(x => X4Database.Instance.X4Size.TryGet(x))
            .Where(x => x is not null)
            .Select(x => x!)
            .OrderBy(x => x);

        EquipmentSizes.AddRange(sizes);
    }


    /// <summary>
    /// 派閥一覧を更新
    /// </summary>
    private void UpdateFactions()
    {
        var checkedFactions = SettingDatabase.Instance.GetCheckedFactionsAtSelectEquipmentWindow();

        // 装備可能な装備の製造元派閥一覧を作成
        var factions = X4Database.Instance.Ware.GetAll<IEquipment>()
            .Where(x => !x.Tags.Contains("noplayerblueprint"))
            .Where(x => _manager.Ware.Equipments.Values.Any(y => y.CanEquipped(x)))
            .SelectMany(x => x.Owners)
            .Distinct()
            .Select(x => new FactionsListItem(Messenger, x, checkedFactions.Contains(x.FactionID)));

        Factions.AddRange(factions);
    }


    /// <summary>
    /// プリセットを初期化
    /// </summary>
    private void InitPreset()
    {
        Presets.AddRange(SettingDatabase.Instance.GetModulePreset(_manager.Ware.ID).Select(x => new PresetComboboxItem(x.ID, x.Name)));
    }


    /// <summary>
    /// 選択サイズ変更時
    /// </summary>
    partial void OnSelectedSizeChanged(IX4Size value)
    {
        foreach (var vm in EquipmentListViewModels)
        {
            vm.UpdateSelectedSize(value);
        }
    }


    /// <summary>
    /// 選択プリセット変更時
    /// </summary>
    partial void OnSelectedPresetChanged(PresetComboboxItem? value)
    {
        if (_removingPreset) return;
        foreach (var vm in EquipmentListViewModels)
        {
            vm.UpdateSelectedPreset(value);
        }
    }


    /// <summary>
    /// チェック状態を保存
    /// </summary>
    public void SaveCheckState()
    {
        var checkedFactions = Factions.Where(x => x.IsChecked).Select(x => x.Faction);
        SettingDatabase.Instance.SetCheckedFactionsAtSelectEquipmentWindow(checkedFactions);
    }



    /// <summary>
    /// プリセット名を編集
    /// </summary>
    public void EditPresetName()
    {
        if (SelectedPreset is null)
        {
            return;
        }

        // 新プリセット名
        var (onOK, newPresetName) = SelectStringDialog.ShowDialog("Lang:RenamePreset_Title", "Lang:RenamePreset_Description", SelectedPreset.Name, IsValidPresetName);
        if (onOK)
        {
            // 新プリセット名が設定された場合
            SettingDatabase.Instance.UpdateModulePresetName(_manager.Ware.ID, SelectedPreset.ID, newPresetName);
            SelectedPreset.Name = newPresetName;
        }
    }


    /// <summary>
    /// プリセット追加
    /// </summary>
    public void AddPreset()
    {
        var (onOK, presetName) = SelectStringDialog.ShowDialog("Lang:SaveNewPreset_Title", "Lang:SaveNewPreset_Description", "", IsValidPresetName);
        if (onOK)
        {
            var newID = SettingDatabase.Instance.GetLastModulePresetsID(_manager.Ware.ID);
            SettingDatabase.Instance.AddModulePreset(
                _manager.Ware.ID,
                newID,
                presetName,
                EquipmentListViewModels.SelectMany(x => x.Equipped).Select(x => x.Equipment)
            );

            var item = new PresetComboboxItem(newID, presetName);
            Presets.Add(item);
            SelectedPreset = item;
        }
    }



    /// <summary>
    /// プリセットを削除
    /// </summary>
    public void DeletePreset()
    {
        if (SelectedPreset is null)
        {
            return;
        }

        var result = _localizedMessageBox.YesNo("Lang:DeletePresetConfirmMessage", "Lang:Common_MessageBoxTitle_Error", LocalizedMessageBoxResult.No, SelectedPreset.Name);
        if (result == LocalizedMessageBoxResult.Yes)
        {
            SettingDatabase.Instance.DeleteModulePreset(_manager.Ware.ID, SelectedPreset.ID);

            _removingPreset = true;
            Presets.Remove(SelectedPreset);
            SelectedPreset = null;
            _removingPreset = false;
        }
    }


    /// <summary>
    /// プリセットを上書き保存する
    /// </summary>
    public void OverwritePreset()
    {
        if (SelectedPreset is not null)
        {
            SettingDatabase.Instance.OverwritePreset(
                _manager.Ware.ID,
                SelectedPreset.ID,
                EquipmentListViewModels.SelectMany(x => x.Equipped).Select(x => x.Equipment)
            );
        }
    }



    /// <summary>
    /// 装備を保存する
    /// </summary>
    public void SaveEquipment()
    {
        _manager.ResetEquipment(EquipmentListViewModels.SelectMany(x => x.Equipped.Select(y => y.Equipment)));
        foreach (var vm in EquipmentListViewModels)
        {
            vm.SetSaved();
        }
    }



    /// <summary>
    /// プリセット名が有効か判定する
    /// </summary>
    /// <param name="presetName">判定対象プリセット名</param>
    /// <returns>プリセット名が有効か</returns>
    private bool IsValidPresetName(string presetName)
    {
        var ret = true;

        if (string.IsNullOrWhiteSpace(presetName))
        {
            _localizedMessageBox.Warn("Lang:InvalidPresetNameMessage", "Lang:Common_MessageBoxTitle_Warning");
            ret = false;
        }

        return ret;
    }
}
