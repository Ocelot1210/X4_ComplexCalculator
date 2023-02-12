using AvalonDock;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows.Input;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;
using X4_ComplexCalculator.Main.Menu.File.Export;
using X4_ComplexCalculator.Main.Menu.File.Import;
using X4_ComplexCalculator.Main.WorkArea.SaveDataWriter;
using X4_ComplexCalculator.Main.WorkArea.UI;
using X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.Menu.Tab;
using X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;
using X4_ComplexCalculator.Main.WorkArea.UI.StationSummary;
using X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;
using X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

namespace X4_ComplexCalculator.Main.WorkArea;

/// <summary>
/// 作業エリア用ViewModel
/// </summary>
public sealed class WorkAreaViewModel : BindableBase, IDisposable
{
    #region メンバ
    /// <summary>
    /// モデル
    /// </summary>
    private readonly WorkAreaModel _model;
    #endregion


    #region プロパティ
    /// <summary>
    /// 表示/非表示用メニューアイテム一覧
    /// </summary>
    public ObservableRangeCollection<VisiblityMenuItem> VisiblityMenuItems => LayoutManager.VisiblityMenuItems;


    /// <summary>
    /// モジュール一覧
    /// </summary>
    public ModulesGridViewModel Modules { get; }


    /// <summary>
    /// 製品一覧
    /// </summary>
    public ProductsGridViewModel Products { get; }


    /// <summary>
    /// 建造リソース一覧
    /// </summary>
    public BuildResourcesGridViewModel Resources { get; }


    /// <summary>
    /// 保管庫一覧
    /// </summary>
    public StoragesGridViewModel Storages { get; }


    /// <summary>
    /// 保管庫割当情報
    /// </summary>
    public StorageAssignViewModel StorageAssign { get; }


    /// <summary>
    /// 概要
    /// </summary>
    public StationSummaryViewModel Summary { get; }


    /// <summary>
    /// 設定
    /// </summary>
    public IStationSettings Settings => _model.StationData.Settings;


    /// <summary>
    /// タブのタイトル文字列
    /// </summary>
    public string Title => _model.Title;


    /// <summary>
    /// アンロード時
    /// </summary>
    public ICommand OnLoadedCommand { get; }


    /// <summary>
    /// モジュールの内容に変更があったか
    /// </summary>
    public bool HasChanged => _model.HasChanged;


    /// <summary>
    /// 保存先ファイルパス
    /// </summary>
    public string SaveFilePath => _model.SaveFilePath;


    /// <summary>
    /// レイアウト管理
    /// </summary>
    public LayoutManager LayoutManager { get; }


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    public ILocalizedMessageBox MessageBox { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="layoutID">レイアウトID</param>
    /// <param name="messageBox">メッセージボックス表示用</param>
    /// <remarks>
    /// レイアウトIDが負の場合、レイアウトは指定されていない事にする
    /// </remarks>
    public WorkAreaViewModel(long layoutID, ILocalizedMessageBox messageBox)
    {
        _model                  = new(new SQLiteSaveDataWriter(messageBox));
        LayoutManager           = new LayoutManager(layoutID);
        MessageBox              = messageBox;

        Summary                 = new(_model.StationData);
        Modules                 = new(_model.StationData, messageBox);
        Products                = new(_model.StationData, messageBox);
        Resources               = new(_model.StationData);
        Storages                = new(_model.StationData);
        StorageAssign           = new(_model.StationData);

        Modules.AutoAddModuleCommand = Products.AutoAddModuleCommand;
        OnLoadedCommand     = new DelegateCommand<DockingManager>(LayoutManager.OnLoaded);

        _model.PropertyChanged += Model_PropertyChanged;
    }


    /// <summary>
    /// インポート実行
    /// </summary>
    /// <param name="import"></param>
    public bool Import(IImport import) => import.Import(_model);


    /// <summary>
    /// エクスポート実行
    /// </summary>
    /// <param name="export"></param>
    public bool Export(IExport export) => export.Export(_model);


    /// <summary>
    /// 上書き保存
    /// </summary>
    public void Save() => _model.Save();


    /// <summary>
    /// 名前を付けて保存
    /// </summary>
    public void SaveAs() => _model.SaveAs();


    /// <summary>
    /// ファイル読み込み
    /// </summary>
    /// <param name="path">ファイルパス</param>
    public bool LoadFile(string path, IProgress<int> progress) => _model.Load(path, progress);


    /// <summary>
    /// Modelのプロパティ変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_model.HasChanged):
                RaisePropertyChanged(nameof(HasChanged));
                RaisePropertyChanged(nameof(Title));
                break;

            case nameof(_model.Title):
                RaisePropertyChanged(nameof(Title));
                break;

            default:
                break;
        }
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose()
    {
        _model.PropertyChanged -= Model_PropertyChanged;
        _model.Dispose();
        LayoutManager.Dispose();
        Summary.Dispose();
        Modules.Dispose();
        Products.Dispose();
        Resources.Dispose();
        Storages.Dispose();
    }
}
