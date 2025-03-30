using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Windows.Data;
using System.Windows.Input;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

public sealed partial class ModulesGridViewModel : ObservableObject, IDisposable
{
    #region メンバ
    /// <summary>
    /// Model
    /// </summary>
    private readonly ModulesGridModel _model;
    #endregion


    #region プロパティ
    /// <summary>
    /// モジュール一覧表示用
    /// </summary>
    public ListCollectionView ModulesView { get; }


    /// <summary>
    /// 検索するモジュール名
    /// </summary>
    [ObservableProperty]
    public partial string SearchModuleName { get; set; } = "";


    /// <summary>
    /// コンテキストメニューの処理
    /// </summary>
    public ContextMenuOperation ContextMenu { get; }


    /// <summary>
    /// セルフォーカス用のコマンド
    /// </summary>
    public ICommand? CellFocusCommand { private get; set; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messanger">メッセージ交換用</param>
    /// <param name="stationData">計算機で使用するステーション情報</param>
    /// <param name="localizedMessageBox">メッセージボックス表示用</param>
    public ModulesGridViewModel(IMessenger messenger, StationData stationData, ILocalizedMessageBox localizedMessageBox)
    {
        _model = new ModulesGridModel(messenger, stationData.ModulesInfo, localizedMessageBox);
        ModulesView = (ListCollectionView)CollectionViewSource.GetDefaultView(_model.Modules);
        ModulesView.Filter = Filter;
        ContextMenu = new ContextMenuOperation(messenger, stationData.ModulesInfo, ModulesView);
    }


    /// <inheritdoc/>
    public void Dispose()
    {
        ContextMenu.Dispose();
        _model.Dispose();
    }


    /// <summary>
    /// 検索モジュール名変更時
    /// </summary>
    partial void OnSearchModuleNameChanged(string value)
    {
        ModulesView.Refresh();
    }


    /// <summary>
    /// モジュール追加
    /// </summary>
    [RelayCommand]
    private void AddModule() => _model.ShowAddModuleWindow();


    /// <summary>
    /// モジュールを置換する
    /// </summary>
    [RelayCommand]
    private void ReplaceModule(ModulesGridItem oldItem)
    {
        if (_model.ReplaceModule(oldItem))
        {
            ModulesView.Refresh();
        }
    }


    /// <summary>
    /// モジュール追加
    /// </summary>
    [RelayCommand]
    private void MergeModule() => _model.MergeModule();


    /// <summary>
    /// 不足モジュールを自動追加
    /// </summary>
    [RelayCommand]
    private void AutoAddModule() => _model.AutoAddModule();


    /// <summary>
    /// フィルタイベント
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private bool Filter(object obj)
    {
        return obj is ModulesGridItem src && (SearchModuleName == "" || 0 <= src.Module.Name.IndexOf(SearchModuleName, StringComparison.InvariantCultureIgnoreCase));
    }
}
