using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Linq;
using System.Reactive.Linq;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;
using X4_ComplexCalculator.Common.Dialog.SelectStringDialog;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Entity;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

/// <summary>
/// 装備編集画面のModel
/// </summary>
class EditEquipmentModel : BindableBase, IDisposable
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
    public ObservableRangeCollection<IX4Size> EquipmentSizes { get; } = new();


    /// <summary>
    /// 種族一覧
    /// </summary>
    public ObservablePropertyChangedCollection<FactionsListItem> Factions { get; } = new();


    /// <summary>
    /// プリセット一覧
    /// </summary>
    public ObservableRangeCollection<PresetComboboxItem> Presets { get; } = new();


    /// <summary>
    /// 選択中のプリセット
    /// </summary>
    public ReactiveProperty<PresetComboboxItem?> SelectedPreset { get; } = new();


    /// <summary>
    /// 選択中のサイズ
    /// </summary>
    public ReactiveProperty<IX4Size> SelectedSize { get; }


    /// <summary>
    /// タブアイテム一覧
    /// </summary>
    public ObservableRangeCollection<EquipmentListViewModel> EquipmentListViewModels { get; } = new();
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="ware">編集対象ウェア</param>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    public EditEquipmentModel(EquippableWareEquipmentManager equipmentManager, ILocalizedMessageBox localizedMessageBox)
    {
        // 初期化
        _manager = equipmentManager;
        _localizedMessageBox = localizedMessageBox;
        InitEquipmentSizes();
        UpdateFactions();
        InitPreset();

        SelectedSize = new ReactiveProperty<IX4Size>(EquipmentSizes.First());
        SelectedSize.Subscribe(x =>
        {
            foreach (var vm in EquipmentListViewModels)
            {
                vm.SelectedSize.Value = x;
            }
        });

        SelectedPreset.Subscribe(x =>
        {
            if (_removingPreset) return;
            foreach (var vm in EquipmentListViewModels)
            {
                vm.SelectedPreset.Value = x;
            }
        });


        {
            string[] types = { "turrets", "shields" };

            var viewModels = types
                .Select(x => X4Database.Instance.EquipmentType.Get(x))
                .Select(x => new EquipmentListModel(equipmentManager, x, SelectedSize.Value))
                .Select(x => new EquipmentListViewModel(x, Factions));

            EquipmentListViewModels.AddRange(viewModels);
        }
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
            .Select(x => new FactionsListItem(x, checkedFactions.Contains(x.FactionID)));

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
        if (SelectedPreset.Value is null)
        {
            return;
        }

        // 新プリセット名
        var (onOK, newPresetName) = SelectStringDialog.ShowDialog("Lang:RenamePreset_Title", "Lang:RenamePreset_Description", SelectedPreset.Value.Name, IsValidPresetName);
        if (onOK)
        {
            // 新プリセット名が設定された場合
            SettingDatabase.Instance.UpdateModulePresetName(_manager.Ware.ID, SelectedPreset.Value.ID, newPresetName);
            SelectedPreset.Value.Name = newPresetName;
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
            SelectedPreset.Value = item;
        }
    }



    /// <summary>
    /// プリセットを削除
    /// </summary>
    public void DeletePreset()
    {
        if (SelectedPreset.Value is null)
        {
            return;
        }

        var result = _localizedMessageBox.YesNo("Lang:DeletePresetConfirmMessage", "Lang:Common_MessageBoxTitle_Error", LocalizedMessageBoxResult.No, SelectedPreset.Value.Name);
        if (result == LocalizedMessageBoxResult.Yes)
        {
            SettingDatabase.Instance.DeleteModulePreset(_manager.Ware.ID, SelectedPreset.Value.ID);

            _removingPreset = true;
            Presets.Remove(SelectedPreset.Value);
            SelectedPreset.Value = null;
            _removingPreset = false;
        }
    }


    /// <summary>
    /// プリセットを上書き保存する
    /// </summary>
    public void OverwritePreset()
    {
        if (SelectedPreset.Value is not null)
        {
            SettingDatabase.Instance.OverwritePreset(
                _manager.Ware.ID,
                SelectedPreset.Value.ID,
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
            vm.Unsaved.Value = false;
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
