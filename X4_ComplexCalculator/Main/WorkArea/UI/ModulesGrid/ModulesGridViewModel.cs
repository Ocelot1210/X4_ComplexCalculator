using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows.Data;
using System.Windows.Input;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

public sealed class ModulesGridViewModel : BindableBase, IDisposable
{
    #region メンバ
    /// <summary>
    /// Model
    /// </summary>
    private readonly ModulesGridModel _model;


    /// <summary>
    /// 検索モジュール名
    /// </summary>
    private string _searchModuleName = "";
    #endregion


    #region プロパティ
    /// <summary>
    /// モジュール一覧表示用
    /// </summary>
    public ListCollectionView ModulesView { get; }


    /// <summary>
    /// 検索するモジュール名
    /// </summary>
    public string SearchModuleName
    {
        get => _searchModuleName;
        set
        {
            if (SetProperty(ref _searchModuleName, value))
            {
                ModulesView.Refresh();
            }
        }
    }

    /// <summary>
    /// コンテキストメニューの処理
    /// </summary>
    public ContextMenuOperation ContextMenu { get; }


    /// <summary>
    /// モジュール追加ボタンクリック
    /// </summary>
    public ICommand AddModuleCommand { get; }


    /// <summary>
    /// モジュール変更
    /// </summary>
    public ICommand ReplaceModuleCommand { get; }


    /// <summary>
    /// セルフォーカス用のコマンド
    /// </summary>
    public ICommand? CellFocusCommand { private get; set; }


    /// <summary>
    /// モジュールマージコマンド
    /// </summary>
    public ICommand MergeModuleCommand { get; }


    /// <summary>
    /// モジュール自動追加コマンド
    /// </summary>
    public ICommand? AutoAddModuleCommand { get; set; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="stationData">計算機で使用するステーション情報</param>
    public ModulesGridViewModel(IStationData stationData)
    {
        _model = new ModulesGridModel(stationData.ModulesInfo);
        ModulesView = (ListCollectionView)CollectionViewSource.GetDefaultView(_model.Modules);
        ModulesView.Filter   = Filter;
        ContextMenu = new ContextMenuOperation(stationData.ModulesInfo, ModulesView);

        AddModuleCommand     = new DelegateCommand(_model.ShowAddModuleWindow);
        MergeModuleCommand   = new DelegateCommand(_model.MergeModule);
        ReplaceModuleCommand = new DelegateCommand<ModulesGridItem>(ReplaceModule);
    }


    /// <inheritdoc/>
    public void Dispose()
    {
        ContextMenu.Dispose();
        _model.Dispose();
    }


    /// <summary>
    /// モジュールを置換する
    /// </summary>
    private void ReplaceModule(ModulesGridItem oldItem)
    {
        if (_model.ReplaceModule(oldItem))
        {
            ModulesView.Refresh();
        }
    }


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
