using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

/// <summary>
/// 装備編集画面のViewModel
/// </summary>
sealed partial class EditEquipmentViewModel : ObservableRecipient, IDisposable
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
    public ICollectionView EquipmentSizesView { get; }


    /// <summary>
    /// 選択中の装備サイズ
    /// </summary>
    public IX4Size SelectedSize
    {
        get => _model.SelectedSize;
        set => _model.SelectedSize = value;
    }


    /// <summary>
    /// 種族一覧
    /// </summary>
    public ICollectionView FactionsView { get; }


    /// <summary>
    /// プリセット一覧
    /// </summary>
    public ICollectionView PresetsView { get; }


    /// <summary>
    /// 選択中のプリセット
    /// </summary>
    public PresetComboboxItem? SelectedPreset
    {
        get => _model.SelectedPreset;
        set => _model.SelectedPreset = value;
    }


    /// <summary>
    /// 編集対象の装備種別一覧
    /// </summary>
    public ICollectionView EquipmentTypesView { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messenger">メッセージ通知用</param>
    /// <param name="equipmentManager">編集対象の装備情報</param>
    /// <param name="messageBox">メッセージボックス表示用</param>
    public EditEquipmentViewModel(IMessenger messenger, EquippableWareEquipmentManager equipmentManager, ILocalizedMessageBox messageBox) : base(messenger)
    {
        ModuleName = equipmentManager.Ware.Name;

        // Model類
        _model = new EditEquipmentModel(messenger, equipmentManager, messageBox);
        _localizedMessageBox = messageBox;


        // その他初期化
        FactionsView = CollectionViewSource.GetDefaultView(_model.Factions);
        FactionsView.SortDescriptions.Clear();
        FactionsView.SortDescriptions.Add(new SortDescription(nameof(FactionsListItem.RaceName), ListSortDirection.Ascending));
        FactionsView.SortDescriptions.Add(new SortDescription(nameof(FactionsListItem.FactionName), ListSortDirection.Ascending));
        FactionsView.GroupDescriptions.Clear();
        FactionsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(FactionsListItem.RaceID)));

        EquipmentSizesView  = CollectionViewSource.GetDefaultView(_model.EquipmentSizes);
        PresetsView         = CollectionViewSource.GetDefaultView(_model.Presets);
        EquipmentTypesView  = CollectionViewSource.GetDefaultView(_model.EquipmentListViewModels);

        Messenger.RegisterPropertyChangedMessage(this, static (EditEquipmentModel x) => x.SelectedSize,   static (r, m) => r.OnPropertyChanged(nameof(SelectedSize)));
        Messenger.RegisterPropertyChangedMessage(this, static (EditEquipmentModel x) => x.SelectedPreset, static (r, m) => r.OnPropertyChanged(nameof(SelectedPreset)));
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _model.Dispose();
        Messenger.UnregisterAll(this);
    }


    /// <summary>
    /// ウィンドウが閉じられる時
    /// </summary>
    [RelayCommand]
    private void OnWindowClosing(CancelEventArgs e)
    {
        // 装備が未保存の場合
        if (_model.EquipmentListViewModels.Any(x => x.Unsaved))
        {
            (string, string?)[] buttons = [
                ("Lang:EditEquipmentWindow_CloseConfirmMessage_Save", null),
                ("Lang:EditEquipmentWindow_CloseConfirmMessage_DontSave", "Lang:EditEquipmentWindow_CloseConfirmMessage_DontSave_Description"),
                ("Lang:EditEquipmentWindow_CloseConfirmMessage_Cancel", "Lang:EditEquipmentWindow_CloseConfirmMessage_Cancel_Description"),
            ];
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
    private void SaveEquipment()
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


    /// <summary>
    /// プリセット削除ボタンクリック時
    /// </summary>
    [RelayCommand]
    private void OnDeletePreset() => _model.DeletePreset();
}
