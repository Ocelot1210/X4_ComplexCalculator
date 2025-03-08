using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule.Entities;
using X4_DataExporterWPF.Entities;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule;

sealed partial class SelectModuleViewModel : ObservableRecipient
{
    #region メンバ
    /// <summary>
    /// モデル
    /// </summary>
    readonly SelectModuleModel _model;


    /// <summary>
    /// 置換モードか
    /// </summary>
    private readonly bool _isReplaceMode;
    #endregion


    #region プロパティ
    /// <summary>
    /// ウィンドウの表示状態
    /// </summary>
    [ObservableProperty]
    public partial bool CloseWindowProperty { get; private set; }


    /// <summary>
    /// 変更前のモジュール名
    /// </summary>
    public string PrevModuleName { get; }


    /// <summary>
    /// モジュール一覧表示用
    /// </summary>
    public ListCollectionView ModulesView { get; }


    /// <summary>
    /// 変更前モジュールを表示するか
    /// </summary>
    public Visibility PrevModuleVisiblity => _isReplaceMode ? Visibility.Visible : Visibility.Collapsed;


    /// <summary>
    /// モジュール種別
    /// </summary>
    public ICollectionView ModuleTypesView { get; }


    /// <summary>
    /// モジュール所有派閥
    /// </summary>
    public ICollectionView ModuleOwnersView { get; }


    /// <summary>
    /// モジュール名検索用
    /// </summary>
    [ObservableProperty]
    public partial string SearchModuleName { get; set; } = "";


    /// <summary>
    /// モジュール一覧ListBoxの選択モード
    /// </summary>
    public SelectionMode ModuleListSelectionMode => _isReplaceMode ? SelectionMode.Single : SelectionMode.Extended;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messanger">メッセージ交換用</param>
    /// <param name="modules">選択結果格納先</param>
    /// <param name="prevModuleName">前回選択されたモジュール名</param>
    public SelectModuleViewModel(IMessenger messenger, ObservableRangeCollection<ModulesGridItem> modules, string prevModuleName = "") : base(messenger)
    {
        _model = new SelectModuleModel(messenger, modules);

        PrevModuleName = prevModuleName;
        _isReplaceMode = prevModuleName != "";

        ModulesView = (ListCollectionView)CollectionViewSource.GetDefaultView(_model.Modules);
        ModulesView.Filter = Filter;
        ModulesView.SortDescriptions.Clear();
        ModulesView.SortDescriptions.Add(new SortDescription(nameof(ModulesListItem.Name), ListSortDirection.Ascending));

        ModuleTypesView = CollectionViewSource.GetDefaultView(_model.ModuleTypes);
        ModuleTypesView.SortDescriptions.Clear();
        ModuleTypesView.SortDescriptions.Add(new SortDescription(nameof(ModuleType.Name), ListSortDirection.Ascending));

        ModuleOwnersView = CollectionViewSource.GetDefaultView(_model.ModuleOwners);
        ModuleOwnersView.SortDescriptions.Clear();
        ModuleOwnersView.SortDescriptions.Add(new SortDescription(nameof(ModuleOwnersListItem.RaceName), ListSortDirection.Ascending));
        ModuleOwnersView.SortDescriptions.Add(new SortDescription(nameof(ModuleOwnersListItem.FactionName), ListSortDirection.Ascending));
        ModuleOwnersView.GroupDescriptions.Clear();
        ModuleOwnersView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ModuleOwnersListItem.RaceID)));


        Messenger.RegisterPropertyChangedMessage(this, static (ModuleTypeListItem x) => x.IsChecked, (r, m) => ModulesView.Refresh());
        Messenger.RegisterPropertyChangedMessage(this, static (ModuleOwnersListItem x) => x.IsChecked, (r, m) => ModulesView.Refresh());

        // 親ウィンドウが閉じられたときに子のウィンドウも閉じるようにする
        Application.Current.MainWindow.Closed += MainWindow_Closed;
    }


    /// <summary>
    /// 親ウィンドウが閉じられた時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        CloseWindowProperty = true;
    }


    /// <summary>
    /// 検索用のモジュール名変更時
    /// </summary>
    partial void OnSearchModuleNameChanged(string value)
    {
        ModulesView.Refresh();
    }


    /// <summary>
    /// ウィンドウが閉じられる時のイベント
    /// </summary>
    [RelayCommand]
    private void WindowClosing(CancelEventArgs _)
    {
        Task.Run(_model.SaveCheckState);

        if (Application.Current.MainWindow is not null)
        {
            Application.Current.MainWindow.Closed -= MainWindow_Closed;
        }

        Messenger.UnregisterPropertyChangedMessage(this, static (ModuleOwnersListItem x) => x.IsChecked);
        Messenger.UnregisterPropertyChangedMessage(this, static (ModulesListItem x) => x.IsChecked);
    }


    /// <summary>
    /// OKボタンクリック時
    /// </summary>
    [RelayCommand]
    private void OKButtonClicked()
    {
        _model.AddSelectedModuleToItemCollection();

        // 置換モードならウィンドウを閉じる
        if (_isReplaceMode)
        {
            CloseWindowProperty = true;
        }
    }


    /// <summary>
    /// ウィンドウを閉じる
    /// </summary>
    [RelayCommand]
    private void CloseWindow()
    {
        CloseWindowProperty = true;
    }


    /// <summary>
    /// フィルタイベント
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private bool Filter(object obj)
    {
        // 表示するか
        var ret = false;

        if (obj is ModulesListItem src)
        {
            using var types = new PooledSet<string>(_model.ModuleTypes.Where(x => x.IsChecked).Select(x => x.ID));
            using var owners = new PooledList<string>(_model.ModuleOwners.Where(x => x.IsChecked).Select(x => x.FactionID));

            var module = X4Database.Instance.Ware.Get<IX4Module>(src.ID);

            ret = true
                && (SearchModuleName == "" || src.Name.Contains(SearchModuleName, StringComparison.InvariantCultureIgnoreCase))
                && types.Contains(module.ModuleType.ModuleTypeID)
                && owners.Intersect(module.Owners.Select(x => x.FactionID)).Any();

            // 非表示になる場合、選択解除(選択解除しないと非表示のモジュールが意図せずモジュール一覧に追加される)
            if (!ret)
            {
                src.IsChecked = false;
            }
        }

        return ret;
    }
}
