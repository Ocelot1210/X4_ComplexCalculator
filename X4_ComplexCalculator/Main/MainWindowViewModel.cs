using AvalonDock;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.Main.Menu.File.Exporters;
using X4_ComplexCalculator.Main.Menu.File.Exporters.StationCalculatorExporter;
using X4_ComplexCalculator.Main.Menu.File.Importers;
using X4_ComplexCalculator.Main.Menu.File.Importers.LoadoutImporters;
using X4_ComplexCalculator.Main.Menu.File.Importers.StationCalculatorImporters;
using X4_ComplexCalculator.Main.Menu.File.Importers.StationPlanImporters;
using X4_ComplexCalculator.Main.Menu.Help;
using X4_ComplexCalculator.Main.Menu.Lang;
using X4_ComplexCalculator.Main.Menu.Layout;
using X4_ComplexCalculator.Main.Menu.View.DBViewers;
using X4_ComplexCalculator.Main.Menu.View.EmpireOverviews;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.SaveDataReaders;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// メイン画面のViewModel
/// </summary>
partial class MainWindowViewModel : ObservableRecipient, IDropTarget
{
    #region メンバ
    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox;


    /// <summary>
    /// メイン画面のModel
    /// </summary>
    private readonly MainWindowModel _model;


    /// <summary>
    /// 作業エリア管理用
    /// </summary>
    private readonly WorkAreaManager _workAreaManager;


    /// <summary>
    /// ヘルプメニューの内容
    /// </summary>
    private readonly HelpMenu _helpMenu;


    /// <summary>
    /// 帝国の概要ウィンドウ
    /// </summary>
    private Window? _empireOverviewWindow;


    /// <summary>
    /// DBビュワーウィンドウ
    /// </summary>
    private Window? _dbViewerWindow;
    #endregion


    #region プロパティ
    /// <summary>
    /// 起動時に更新を確認するかのチェック状態
    /// </summary>
    [ObservableProperty]
    public partial bool CheckUpdateAtLaunch { get; set; }


    /// <summary>
    /// ワークエリア一覧
    /// </summary>
    public ObservableCollection<WorkAreaViewModel> Documents => _workAreaManager.Documents;


    /// <summary>
    /// アクティブなワークスペース
    /// </summary>
    public WorkAreaViewModel? ActiveContent
    {
        set => _workAreaManager.ActiveContent = value;
        get => _workAreaManager.ActiveContent;
    }


    /// <summary>
    /// レイアウト一覧
    /// </summary>
    public ObservableCollection<LayoutMenuItem> Layouts => _workAreaManager.Layouts;


    /// <summary>
    /// インポート処理一覧
    /// </summary>
    public List<IImporter> Importers { get; }


    /// <summary>
    /// エクスポート処理一覧
    /// </summary>
    public List<IExporter> Exporters { get; }


    /// <summary>
    /// 言語一覧
    /// </summary>
    public IReadOnlyList<LangMenuItem> Languages { get; } = LangMenuItem.CreateItems();


    /// <summary>
    /// 保存ファイル読み込み時の進捗表示用
    /// </summary>
    public SaveDataReaderProgress SaveDataReaderProgress { get; } = new();
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messenger">メッセージ通知用</param>
    /// <param name="messageBox">メッセージボックス表示用</param>
    public MainWindowViewModel(IMessenger messenger, ILocalizedMessageBox messageBox) : base(messenger)
    {
        _localizedMessageBox = messageBox;
        _workAreaManager     = new(Messenger, _localizedMessageBox, SaveDataReaderProgress);
        _model               = new(_workAreaManager, _localizedMessageBox);
        _helpMenu            = new HelpMenu(_localizedMessageBox);
        CheckUpdateAtLaunch  = Configuration.Instance.CheckUpdateAtLaunch;

        Importers =
        [
            new StationCalculatorImporter(_workAreaManager, _localizedMessageBox),
            new StationPlanImporter(_workAreaManager, _localizedMessageBox),
            new LoadoutImporter(),
            //new SaveDataImport(new DelegateCommand<IImport>(_Model.Import))   // 作成中のため未リリース
        ];

        Exporters =
        [
            new StationCalculatorExporter(_workAreaManager)
        ];

        Messenger.RegisterPropertyChangedMessage(this, static (WorkAreaManager x) => x.ActiveContent, static (r, m) => r.OnPropertyChanged(nameof(ActiveContent)));
    }


    /// <summary>
    /// ドラッグ中
    /// </summary>
    /// <param name="dropInfo"></param>
    public void DragOver(IDropInfo dropInfo)
    {
        dropInfo.Effects = DragDropEffects.Copy;
    }


    /// <summary>
    /// ドロップされた時
    /// </summary>
    /// <param name="dropInfo"></param>
    public void Drop(IDropInfo dropInfo)
    {
        var paths = ((DataObject)dropInfo.Data).GetFileDropList().OfType<string>();
        _ = _model.OpenFilesAsync(paths);
    }


    /// <summary>
    /// 新規作成
    /// </summary>
    [RelayCommand]
    private void CreateNew() => _workAreaManager.CreateNewDocument();


    /// <summary>
    /// 開く
    /// </summary>
    [RelayCommand]
    private Task OpenAsync() => _workAreaManager.OpenAsync();


    /// <summary>
    /// 保存
    /// </summary>
    [RelayCommand]
    private void Save() => _workAreaManager.SaveDocument();


    /// <summary>
    /// 名前を付けて保存
    /// </summary>
    [RelayCommand]
    private void SaveAs() => _workAreaManager.SaveAsDocument();


    /// <summary>
    /// 問題を報告
    /// </summary>
    [RelayCommand]
    private void ReportIssue() => _helpMenu.ReportIssue();


    /// <summary>
    /// 更新確認ON/OFF
    /// </summary>
    [RelayCommand]
    private void SetCheckUpdateAtLaunch()
    {
        _helpMenu.SetCheckUpdateAtLaunch();
        CheckUpdateAtLaunch = Configuration.Instance.CheckUpdateAtLaunch;
    }


    /// <summary>
    /// 更新を確認...
    /// </summary>
    [RelayCommand]
    private void CheckUpdate() => _ = _helpMenu.CheckUpdateAsync(true);


    /// <summary>
    /// バージョン情報
    /// </summary>
    [RelayCommand]
    private void ShowVersionInfo() => _helpMenu.ShowVersionInfo();


    /// <summary>
    /// ウィンドウがロードされた時
    /// </summary>
    [RelayCommand]
    private async Task WindowLoadedAsync()
    {
        try
        {
            // DB接続開始
            await _model.InitAsync();
            _workAreaManager.Init();

            // 更新チェックが有効な場合のみ更新を確認する
            if (Configuration.Instance.CheckUpdateAtLaunch)
            {
                _ = _helpMenu.CheckUpdateAsync(false);
            }
        }
        catch (Exception e)
        {
            _localizedMessageBox.Error("Lang:MainWindow_UnexpectedErrorMessage", "Lang:Common_MessageBoxTitle_Error", e.Message, e.StackTrace ?? "");
            Environment.Exit(-1);
        }
    }


    /// <summary>
    /// ウィンドウが閉じられる時
    /// </summary>
    [RelayCommand]
    private void WindowClosing(CancelEventArgs e)
    {
        e.Cancel = _model.WindowClosing();
        if (!e.Cancel)
        {
            _empireOverviewWindow?.Close();
            _dbViewerWindow?.Close();

            _helpMenu.Dispose();
        }
    }


    /// <summary>
    /// タブが閉じられる時
    /// </summary>
    /// <param name="e"></param>
    [RelayCommand]
    private void DocumentClosing(DocumentClosingEventArgs e)
    {
        if (e.Document.Content is WorkAreaViewModel workArea)
        {
            e.Cancel = _workAreaManager.DocumentClosing(workArea);
        }
    }


    /// <summary>
    /// 帝国の概要ウィンドウを開く
    /// </summary>
    [RelayCommand]
    private void OpenEmpireOverviewWindow()
    {
        if (_empireOverviewWindow is null)
        {
            _empireOverviewWindow = new EmpireOverviewWindow(Documents);
            _empireOverviewWindow.Closed += (obj, e) => { _empireOverviewWindow = null; };
            _empireOverviewWindow.Show();
        }

        _empireOverviewWindow.Activate();
    }


    /// <summary>
    /// DBビュワーウィンドウを開く
    /// </summary>
    [RelayCommand]
    private void OpenDBViewerWindow()
    {
        if (_dbViewerWindow is null)
        {
            _dbViewerWindow = new DBViewerWindow();
            _dbViewerWindow.Closed += (_, _) => { _dbViewerWindow = null; };
            _dbViewerWindow.Show();
        }

        _dbViewerWindow.Activate();
    }


    /// <summary>
    /// レイアウト保存
    /// </summary>
    [RelayCommand]
    private void SaveLayout() => _workAreaManager.SaveLayout();


    /// <summary>
    /// DB 更新
    /// </summary>
    [RelayCommand]
    private void UpdateDB() => _model.UpdateDB();
}
