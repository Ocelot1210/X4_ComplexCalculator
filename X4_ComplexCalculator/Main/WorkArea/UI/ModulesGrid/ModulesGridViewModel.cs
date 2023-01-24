using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows.Data;
using System.Windows.Input;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;

public class ModulesGridViewModel : BindableBase, IDisposable
{
    #region メンバ
    /// <summary>
    /// Model
    /// </summary>
    private readonly ModulesGridModel _Model;


    /// <summary>
    /// 検索モジュール名
    /// </summary>
    private string _SearchModuleName = "";
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
        get => _SearchModuleName;
        set
        {
            if (SetProperty(ref _SearchModuleName, value))
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
        _Model = new ModulesGridModel(stationData.ModulesInfo);
        ModulesView = (ListCollectionView)CollectionViewSource.GetDefaultView(_Model.Modules);
        ModulesView.Filter   = Filter;
        ContextMenu = new ContextMenuOperation(stationData.ModulesInfo, ModulesView);

        AddModuleCommand     = new DelegateCommand(_Model.ShowAddModuleWindow);
        MergeModuleCommand   = new DelegateCommand(_Model.MergeModule);
        ReplaceModuleCommand = new DelegateCommand<ModulesGridItem>(ReplaceModule);
    }


    /// <inheritdoc/>
    public void Dispose()
    {
        ContextMenu.Dispose();
        _Model.Dispose();
    }


    /// <summary>
    /// モジュールを置換する
    /// </summary>
    private void ReplaceModule(ModulesGridItem oldItem)
    {
        if (_Model.ReplaceModule(oldItem))
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
