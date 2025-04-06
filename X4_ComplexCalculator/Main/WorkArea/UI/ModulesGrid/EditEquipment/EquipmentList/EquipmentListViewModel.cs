using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList;

/// <summary>
/// 装備一覧用ViewModel
/// </summary>
sealed partial class EquipmentListViewModel : ObservableRecipient, IDisposable
{
    #region メンバ
    /// <summary>
    /// 装備一覧用Model
    /// </summary>
    private readonly EquipmentListModel _model;
    #endregion


    #region プロパティ
    /// <summary>
    /// タイトル文字列
    /// </summary>
    public string Title => _model.Title;


    /// <summary>
    /// 装備一覧表示用
    /// </summary>
    public ListCollectionView EquipmentsView { get; }


    /// <summary>
    /// 装備中の装備
    /// </summary>
    public ObservableCollection<EquipmentListItem> Equipped => _model.Equipped;


    /// <summary>
    /// 装備中の装備
    /// </summary>
    public ListCollectionView EquippedView { get; }


    /// <summary>
    /// 装備可能な個数
    /// </summary>
    public int MaxAmount => _model.MaxAmount;


    /// <summary>
    /// 現在装備中の個数
    /// </summary>
    public int EquippedCount => _model.EquippedCount;


    /// <summary>
    /// 装備の検索文字列
    /// </summary>
    [ObservableProperty]
    public partial string SearchEquipmentName { get; set; } = "";


    /// <summary>
    /// 未保存か
    /// </summary>
    public bool Unsaved => _model.Unsaved;
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messenger">メッセージ通知用</param>
    /// <param name="model">装備一覧用Model</param>
    /// <param name="factions">派閥一覧</param>
    public EquipmentListViewModel(
        IMessenger messenger,
        EquipmentListModel model
    ) : base(messenger)
    {
        _model = model;

        EquipmentsView = (ListCollectionView)CollectionViewSource.GetDefaultView(model.Equippable);
        EquipmentsView.Filter = EquipmentsFilter;

        EquippedView = (ListCollectionView)CollectionViewSource.GetDefaultView(model.Equipped);
        EquippedView.Filter = EquippedFilter;

        // 派閥のチェック変更時に表示更新
        Messenger.RegisterPropertyChangedMessage(this, static (FactionsListItem x) => x.IsChecked, static (r, m) => r.EquipmentsView.Refresh());

        // Modelとのプロパティ同期
        Messenger.RegisterPropertyChangedMessage(this, static (EquipmentListModel x) => x.Unsaved,       static (r, m) => r.OnPropertyChanged(nameof(Unsaved)));
        Messenger.RegisterPropertyChangedMessage(this, static (EquipmentListModel x) => x.MaxAmount,     static (r, m) => r.OnMaxAmountChanged());
        Messenger.RegisterPropertyChangedMessage(this, static (EquipmentListModel x) => x.EquippedCount, static (r, m) => r.OnEquippedCountChanged());

        IsActive = true;
    }


    /// <summary>
    /// 検索文字列変更時
    /// </summary>
    partial void OnSearchEquipmentNameChanged(string value) => EquipmentsView.Refresh();


    /// <summary>
    /// 選択サイズ更新
    /// </summary>
    public void UpdateSelectedSize(IX4Size size)
    {
        _model.SelectedSize = size;
        EquipmentsView.Refresh(); 
        EquippedView.Refresh();
    }
    

    /// <summary>
    /// 選択プリセット更新
    /// </summary>
    public void UpdateSelectedPreset(PresetComboboxItem? preset) => _model.SelectedPreset = preset;

    /// <summary>
    /// 保存済みにする
    /// </summary>
    public void SetSaved() => _model.Unsaved = false;


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose() => Messenger.UnregisterAll(this);


    /// <summary>
    /// 装備可能な個数変更時
    /// </summary>
    private void OnMaxAmountChanged()
    {
        OnPropertyChanged(nameof(MaxAmount));
        AddButtonClickedCommand.NotifyCanExecuteChanged();
    }


    /// <summary>
    /// 装備済み個数変更時
    /// </summary>
    private void OnEquippedCountChanged()
    {
        OnPropertyChanged(nameof(EquippedCount));
        AddButtonClickedCommand.NotifyCanExecuteChanged();
        RemoveButtonClickedCommand.NotifyCanExecuteChanged();
    }


    /// <summary>
    /// 追加ボタンクリック時
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void OnAddButtonClicked() => _model.AddSelectedEquipments(Keyboard.IsKeyDown(Key.LeftShift));

    /// <summary>
    /// 追加実行可能か
    /// </summary>
    private bool CanAdd() => EquippedCount < MaxAmount;


    /// <summary>
    /// 削除ボタンクリック時
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRemove))]
    void OnRemoveButtonClicked() => _model.RemoveSelectedEquipments();

    /// <summary>
    /// 削除可能か
    /// </summary>
    private bool CanRemove() => 0 < EquippedCount;



    /// <summary>
    /// フィルタイベント(装備済み用)
    /// </summary>
    /// <param name="obj">評価対象</param>
    /// <returns>評価対象を表示するか</returns>
    private bool EquippedFilter(object obj)
    {
        if (obj is EquipmentListItem item)
        {
            item.IsSelected = false;

            // サイズ違いなら表示しない
            if (!item.Equipment.EquipmentTags.Contains(_model.SelectedSize.SizeID))
            {
                return false;
            }

            return true;
        }

        return false;
    }


    /// <summary>
    /// フィルタイベント(装備一覧用)
    /// </summary>
    /// <param name="obj">評価対象</param>
    /// <returns>評価対象を表示するか</returns>
    private bool EquipmentsFilter(object obj)
    {
        if (obj is EquipmentListItem item)
        {
            item.IsSelected = false;

            // サイズ違いなら表示しない
            if (!item.Equipment.EquipmentTags.Contains(_model.SelectedSize.SizeID))
            {
                return false;
            }

            // 所有派閥でなければ表示しない
            if (!item.Equipment.Owners.Intersect(_model.Factions.Where(x => x.IsChecked).Select(x => x.Faction)).Any())
            {
                return false;
            }

            // フィルタが空なら表示する
            if (SearchEquipmentName == "")
            {
                return true;
            }

            return 0 <= item.Equipment.Name.IndexOf(SearchEquipmentName, StringComparison.InvariantCultureIgnoreCase);
        }

        return false;
    }
}
