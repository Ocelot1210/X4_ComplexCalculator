using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Entities;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

/// <summary>
/// 装備編集画面のViewModel
/// </summary>
sealed partial class EditEquipmentViewModel : ObservableObject, IDisposable
{
    #region メンバ
    /// <summary>
    /// 装備編集画面のModel
    /// </summary>
    private readonly EditEquipmentModel _model;


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox;


    /// <summary>
    /// ゴミ箱
    /// </summary>
    private readonly CompositeDisposable _disposables = new();
    #endregion


    #region プロパティ
    /// <summary>
    /// 編集対象モジュール名
    /// </summary>
    public string ModuleName { get; }


    /// <summary>
    /// ウィンドウの表示状態
    /// </summary>
    [ObservableProperty]
    public partial bool CloseWindowProperty { get; set; }


    /// <summary>
    /// 装備サイズ一覧
    /// </summary>
    public ObservableCollection<IX4Size> EquipmentSizes => _model.EquipmentSizes;


    /// <summary>
    /// 選択中の装備サイズ
    /// </summary>
    public ReactiveProperty<IX4Size> SelectedSize { get; }


    /// <summary>
    /// 種族一覧
    /// </summary>
    public ICollectionView FactionsView { get; }


    /// <summary>
    /// プリセット
    /// </summary>
    public ObservableCollection<PresetComboboxItem> Presets => _model.Presets;


    /// <summary>
    /// 選択中のプリセット
    /// </summary>
    public ReactiveProperty<PresetComboboxItem?> SelectedPreset { get; }


    /// <summary>
    /// タブアイテム一覧
    /// </summary>
    public ObservableCollection<EquipmentListViewModel> EquipmentListViewModels => _model.EquipmentListViewModels;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="equipmentManager">編集対象の装備情報</param>
    /// <param name="messageBox">メッセージボックス表示用</param>
    public EditEquipmentViewModel(EquippableWareEquipmentManager equipmentManager, ILocalizedMessageBox messageBox)
    {
        ModuleName = equipmentManager.Ware.Name;

        // Model類
        _model = new EditEquipmentModel(equipmentManager, messageBox);
        _localizedMessageBox = messageBox;


        // その他初期化
        SelectedSize = _model.SelectedSize
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .AddTo(_disposables);

        SelectedPreset = _model.SelectedPreset
            .ToReactivePropertyAsSynchronized(x => x.Value)
            .AddTo(_disposables);


        FactionsView = CollectionViewSource.GetDefaultView(_model.Factions);
        FactionsView.SortDescriptions.Clear();
        FactionsView.SortDescriptions.Add(new SortDescription(nameof(FactionsListItem.RaceName), ListSortDirection.Ascending));
        FactionsView.SortDescriptions.Add(new SortDescription(nameof(FactionsListItem.FactionName), ListSortDirection.Ascending));
        FactionsView.GroupDescriptions.Clear();
        FactionsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(FactionsListItem.RaceID)));
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _model.Dispose();
        _disposables.Dispose();
    }


    /// <summary>
    /// ウィンドウが閉じられる時
    /// </summary>
    [RelayCommand]
    private void OnWindowClosing(CancelEventArgs e)
    {
        // 装備が未保存の場合
        if (EquipmentListViewModels.Any(x => x.Unsaved.Value))
        {
            (string, string?)[] buttons = {
                ("Lang:EditEquipmentWindow_CloseConfirmMessage_Save", null),
                ("Lang:EditEquipmentWindow_CloseConfirmMessage_DontSave", "Lang:EditEquipmentWindow_CloseConfirmMessage_DontSave_Description"),
                ("Lang:EditEquipmentWindow_CloseConfirmMessage_Cancel", "Lang:EditEquipmentWindow_CloseConfirmMessage_Cancel_Description"),
            };
            var result = _localizedMessageBox.MultiChoiceInfo("Lang:EditEquipmentWindow_CloseConfirmMessage", "Lang:Common_MessageBoxTitle_Confirmation", buttons, 2);
            switch (result)
            {
                // 保存する場合
                case 0:
                    _model.SaveEquipment();
                    break;

                // 保存せずに閉じる場合
                case 1:
                    break;

                // キャンセルする場合
                default:
                    CloseWindowProperty = false;
                    e.Cancel = true;
                    break;
            }
        }

        // ウィンドウを閉じる場合、チェック状態を保存
        if (!e.Cancel)
        {
            Task.Run(_model.SaveCheckState);
            Dispose();
        }
    }


    /// <summary>
    /// 保存ボタンクリック時
    /// </summary>
    [RelayCommand]
    private void OnSavebuttonClicked()
    {
        _model.SaveEquipment();
        CloseWindowProperty = true;
    }


    /// <summary>
    /// 閉じるボタンクリック時
    /// </summary>
    [RelayCommand]
    private void OnCloseWindow() => CloseWindowProperty = true;


    /// <summary>
    /// プリセット保存ボタンクリック時
    /// </summary>
    [RelayCommand]
    private void OnOverwritePreset() => _model.OverwritePreset();


    /// <summary>
    /// プリセット編集ボタンクリック時
    /// </summary>
    [RelayCommand]
    private void OnEditPresetName() => _model.EditPresetName();


    /// <summary>
    /// プリセット追加ボタンクリック時
    /// </summary>
    [RelayCommand]
    private void OnAddPreset() => _model.AddPreset();
}
