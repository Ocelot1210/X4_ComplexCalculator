using AvalonDock;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;
using X4_ComplexCalculator.Infrastructure;
using X4_ComplexCalculator.Main.Menu.File.Export;
using X4_ComplexCalculator.Main.Menu.File.Import;
using X4_ComplexCalculator.Main.Menu.File.Import.LoadoutImport;
using X4_ComplexCalculator.Main.Menu.File.Import.StationPlanImport;
using X4_ComplexCalculator.Main.Menu.Lang;
using X4_ComplexCalculator.Main.Menu.Layout;
using X4_ComplexCalculator.Main.Menu.View.DBViewer;
using X4_ComplexCalculator.Main.Menu.View.EmpireOverview;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// メイン画面のViewModel
/// </summary>
partial class MainWindowViewModel : ObservableObject, IDropTarget
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
    /// 言語一覧管理用
    /// </summary>
    private readonly LanguagesManager _langMgr = new();


    /// <summary>
    /// 作業エリア管理用
    /// </summary>
    private readonly WorkAreaManager _workAreaManager;


    /// <summary>
    /// 作業エリアファイル読み書き用
    /// </summary>
    private readonly WorkAreaFileIO _workAreaFileIO;


    /// <summary>
    /// 帝国の概要ウィンドウ
    /// </summary>
    private Window? _empireOverviewWindow;


    /// <summary>
    /// DBビュワーウィンドウ
    /// </summary>
    private Window? _dbViewerWindow;


    /// <summary>
    /// アップデート機能
    /// </summary>
    private readonly ApplicationUpdater _applicationUpdater = new();
    #endregion


    #region プロパティ
    /// <summary>
    /// 起動時に更新を確認するかのチェック状態
    /// </summary>
    [ObservableProperty]
    public partial bool CheckUpdateAtLaunch { get; private set; }


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
    public List<IImport> Imports { get; }


    /// <summary>
    /// エクスポート処理一覧
    /// </summary>
    public List<IExport> Exports { get; }


    /// <summary>
    /// 言語一覧
    /// </summary>
    public IReadOnlyList<LangMenuItem> Languages => _langMgr.Languages;


    /// <summary>
    /// ファイル読み込みがビジー状態か
    /// </summary>
    public bool FileLoadingIsBusy => _workAreaFileIO.IsBusy;


    /// <summary>
    /// ファイル読み込み進捗
    /// </summary>
    public int FileLoadingProgress => _workAreaFileIO.Progress;


    /// <summary>
    /// 読込中のファイル名
    /// </summary>
    public string LoadingFileName => _workAreaFileIO.LoadingFileName;
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messageBox">メッセージボックス表示用</param>
    public MainWindowViewModel(ILocalizedMessageBox messageBox)
    {
        _localizedMessageBox             = messageBox;
        _workAreaManager                 = new(_localizedMessageBox);
        _workAreaFileIO                  = new(_workAreaManager, _localizedMessageBox);
        _model                           = new(_workAreaManager, _workAreaFileIO, _localizedMessageBox);
        CheckUpdateAtLaunch              = Configuration.Instance.CheckUpdateAtLaunch;
        _workAreaFileIO.PropertyChanged += Member_PropertyChanged;

        Imports = new List<IImport>()
        {
            new StationCalculatorImport(_workAreaManager, _localizedMessageBox),
            new StationPlanImport(_workAreaManager, _localizedMessageBox),
            new LoadoutImport(),
            //new SaveDataImport(new DelegateCommand<IImport>(_Model.Import))   // 作成中のため未リリース
        };

        Exports = new List<IExport>()
        {
            new StationCalculatorExport(_workAreaManager)
        };
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
        _model.OpenFiles(paths);
    }


    /// <summary>
    /// メンバのプロパティ変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Member_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_workAreaFileIO.IsBusy):
                OnPropertyChanged(nameof(FileLoadingIsBusy));
                break;

            case nameof(_workAreaFileIO.Progress):
                OnPropertyChanged(nameof(FileLoadingProgress));
                break;

            case nameof(_workAreaFileIO.LoadingFileName):
                OnPropertyChanged(nameof(LoadingFileName));
                break;

            default:
                break;
        }
    }


    /// <summary>
    /// 新規作成
    /// </summary>
    [RelayCommand]
    private void CreateNew()
    {
        _workAreaFileIO.CreateNew();
        OnPropertyChanged(nameof(ActiveContent));
    }


    /// <summary>
    /// 開く
    /// </summary>
    [RelayCommand]
    private void Open()
    {
        _workAreaFileIO.Open();
        OnPropertyChanged(nameof(ActiveContent));
    }


    /// <summary>
    /// 保存
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        _workAreaManager.ActiveContent?.Save();
    }


    /// <summary>
    /// 名前を付けて保存
    /// </summary>
    [RelayCommand]
    private void SaveAs()
    {
        _workAreaManager.ActiveContent?.SaveAs();
    }


    /// <summary>
    /// 問題を報告
    /// </summary>
    [RelayCommand]
    private void ReportIssue()
    {
        string url = ThisAssembly.Git.RepositoryUrl + "/issues";
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }


    /// <summary>
    /// 更新確認ON/OFF
    /// </summary>
    [RelayCommand]
    private void SetCheckUpdateAtLaunch()
    {
        Configuration.Instance.CheckUpdateAtLaunch = !Configuration.Instance.CheckUpdateAtLaunch;
        CheckUpdateAtLaunch = Configuration.Instance.CheckUpdateAtLaunch;
    }


    /// <summary>
    /// 更新を確認...
    /// </summary>
    [RelayCommand]
    private async Task CheckUpdateAsync(bool isUserOperation = false)
    {
        if (_applicationUpdater.FinishedDownload && isUserOperation)
        {
            _localizedMessageBox.Ok("Lang:CheckUpdate_FinishedDownloadDescription", "Lang:CheckUpdate_Title");
            return;
        }
        else if (_applicationUpdater.NowDownloading && isUserOperation)
        {
            _localizedMessageBox.Ok("Lang:CheckUpdate_StartDownloadDescription", "Lang:CheckUpdate_Title");
            return;
        }

        string? latestVersion;
        try
        {
            latestVersion = await _applicationUpdater.CheckUpdate();
        }
        catch (HttpRequestException)
        {
            if (isUserOperation)
            {
                _localizedMessageBox.Error("Lang:CheckUpdate_FailedDescription", "Lang:CheckUpdate_Title");
            }
            return;
        }
        if (latestVersion is null)
        {
            if (isUserOperation)
            {
                _localizedMessageBox.Ok("Lang:CheckUpdate_NoUpdateDescription", "Lang:CheckUpdate_Title", VersionInfo.BASE_VERSION);
            }
            return;
        }

        var result = _localizedMessageBox.YesNo("Lang:CheckUpdate_HasUpdateDescription", "Lang:CheckUpdate_Title", LocalizedMessageBoxResult.Yes, VersionInfo.BASE_VERSION, latestVersion);

        if (result != LocalizedMessageBoxResult.Yes) return;

        _applicationUpdater.StartDownloadByBackground();
        _localizedMessageBox.Ok("Lang:CheckUpdate_StartDownloadDescription", "Lang:CheckUpdate_Title");
    }


    /// <summary>
    /// バージョン情報
    /// </summary>
    [RelayCommand]
    private void ShowVersionInfo()
    {
        const string VERSION = VersionInfo.DETAIL_VERSION;
        const string COMMIT = ThisAssembly.Git.Sha;
        const string DATE = ThisAssembly.Git.CommitDate;
        var dotnetVersion = Environment.Version.ToString();

        _localizedMessageBox.Ok("Lang:MainWindow_Menu_Help_VersionInfo_MessageDescription", "Lang:MainWindow_Menu_Help_VersionInfo_MessageTitle", VERSION, COMMIT, DATE, dotnetVersion);
    }


    /// <summary>
    /// ウィンドウがロードされた時
    /// </summary>
    [RelayCommand]
    private void WindowLoaded()
    {
        try
        {
            // DB接続開始
            _model.Init();
            _workAreaManager.Init();

            // 更新チェックが有効な場合のみ更新を確認する
            if (Configuration.Instance.CheckUpdateAtLaunch)
            {
                CheckUpdateCommand.Execute(false);
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

            if (_applicationUpdater.FinishedDownload) _applicationUpdater.Update();
            else if (_applicationUpdater.NowDownloading)
            {
                var dialog = new UpdateDownloadProglessDialog
                {
                    DataContext = new UpdateDownloadProgressViewModel(_applicationUpdater)
                };
                dialog.Show();
            }
            _workAreaManager.Dispose();
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
    private void SaveLayout()
    {
        _workAreaManager.SaveLayout();
    }


    /// <summary>
    /// DB 更新
    /// </summary>
    [RelayCommand]
    private void UpdateDB()
    {
        _model.UpdateDB();
    }
}
