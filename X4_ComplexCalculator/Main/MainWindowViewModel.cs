using AvalonDock;
using GongSolutions.Wpf.DragDrop;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
class MainWindowViewModel : BindableBase, IDropTarget
{
    #region メンバ
    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox;

    /// <summary>
    /// メイン画面のModel
    /// </summary>
    private readonly MainWindowModel _mainWindowModel;


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
    /// インポート/エクスポート処理用
    /// </summary>
    private readonly ImportExporter _importExporter;


    /// <summary>
    /// 帝国の概要ウィンドウ
    /// </summary>
    private Window? _empireOverviewWindow;


    /// <summary>
    /// DBビュワーウィンドウ
    /// </summary>
    private Window? _dBViewerWindow;


    /// <summary>
    /// アップデート機能
    /// </summary>
    private readonly ApplicationUpdater _applicationUpdater = new();
    #endregion


    #region プロパティ
    /// <summary>
    /// Windowがロードされた時
    /// </summary>
    public ICommand WindowLoadedCommand { get; }

    /// <summary>
    /// Windowが閉じられる時
    /// </summary>
    public ICommand WindowClosingCommand { get; }


    /// <summary>
    /// レイアウト保存
    /// </summary>
    public ICommand SaveLayout { get; }


    /// <summary>
    /// 新規作成
    /// </summary>
    public ICommand CreateNewCommand { get; }


    /// <summary>
    /// 上書き保存
    /// </summary>
    public ICommand SaveCommand { get; }


    /// <summary>
    /// 名前を指定して保存
    /// </summary>
    public ICommand SaveAsCommand { get; }


    /// <summary>
    /// 開く
    /// </summary>
    public ICommand OpenCommand { get; }


    /// <summary>
    /// 帝国の概要ウィンドウを開く
    /// </summary>
    public ICommand OpenEmpireOverviewWindowCommand { get; }


    /// <summary>
    /// DBビュワーウィンドウを開く
    /// </summary>
    public ICommand OpenDBViewerWindowCommand { get; }


    /// <summary>
    /// DB更新
    /// </summary>
    public ICommand UpdateDBCommand { get; }


    /// <summary>
    /// 問題を報告
    /// </summary>
    public ICommand ReportIssueCommand { get; }


    /// <summary>
    /// 起動時に更新を確認するかのチェック状態
    /// </summary>
    public ReactiveProperty<bool> CheckUpdateAtLaunch { get; }


    /// <summary>
    /// 起動時に更新を確認するか
    /// </summary>
    public ICommand SetCheckUpdateAtLaunchCommand { get; }


    /// <summary>
    /// 更新を確認...
    /// </summary>
    public AsyncReactiveCommand<bool> CheckUpdateCommand { get; }


    /// <summary>
    /// バージョン情報
    /// </summary>
    public ICommand VersionInfoCommand { get; }


    /// <summary>
    /// タブが閉じられる時
    /// </summary>
    public ICommand DocumentClosingCommand { get; }


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
        _workAreaFileIO                  = new(_workAreaManager, messageBox);
        _mainWindowModel                 = new(_workAreaManager, _workAreaFileIO, _localizedMessageBox);
        WindowLoadedCommand              = new DelegateCommand(WindowLoaded);
        WindowClosingCommand             = new DelegateCommand<CancelEventArgs>(WindowClosing);
        CreateNewCommand                 = new DelegateCommand(CreateNew);
        SaveLayout                       = new DelegateCommand(_workAreaManager.SaveLayout);
        SaveCommand                      = new DelegateCommand(_workAreaFileIO.Save);
        SaveAsCommand                    = new DelegateCommand(_workAreaFileIO.SaveAs);
        OpenCommand                      = new DelegateCommand(Open);
        UpdateDBCommand                  = new DelegateCommand(_mainWindowModel.UpdateDB);
        ReportIssueCommand               = new DelegateCommand(ReportIssue);
        CheckUpdateAtLaunch              = new ReactiveProperty<bool>(Configuration.Instance.CheckUpdateAtLaunch);
        SetCheckUpdateAtLaunchCommand    = new DelegateCommand(SetCheckUpdateAtLaunch);
        CheckUpdateCommand               = new AsyncReactiveCommand<bool>().WithSubscribe(CheckUpdate);
        VersionInfoCommand               = new DelegateCommand(ShowVersionInfo);
        DocumentClosingCommand           = new DelegateCommand<DocumentClosingEventArgs>(DocumentClosing);
        OpenEmpireOverviewWindowCommand  = new DelegateCommand(OpenEmpireOverviewWindow);
        OpenDBViewerWindowCommand        = new DelegateCommand(OpenDBViewerWindow);
        _workAreaFileIO.PropertyChanged += Member_PropertyChanged;

        _importExporter = new ImportExporter(_workAreaManager, _localizedMessageBox);
        Imports = new List<IImport>()
        {
            new StationCalculatorImport(new DelegateCommand<IImport>(_importExporter.Import), messageBox),
            new StationPlanImport(new DelegateCommand<IImport>(_importExporter.Import)),
            new LoadoutImport(new DelegateCommand<IImport>(_importExporter.Import)),
            //new SaveDataImport(new DelegateCommand<IImport>(_Model.Import))   // 作成中のため未リリース
        };

        Exports = new List<IExport>()
        {
            new StationCalculatorExport(new DelegateCommand<IExport>(_importExporter.Export))
        };
    }


    /// <summary>
    /// ファイル/フォルダ内の.x4ファイルを列挙する
    /// </summary>
    /// <param name="pathes">ファイル/フォルダパス</param>
    /// <param name="maxRecursion">最大再帰回数</param>
    /// <param name="currRecursion">現在の再帰回数</param>
    /// <returns></returns>
    private IEnumerable<string> GetX4Files(IEnumerable<string> pathes, int maxRecursion, int currRecursion = 0)
    {
        // 再帰最大の場合、何もしない
        if (maxRecursion < currRecursion)
        {
            yield break;
        }

        foreach (var path in pathes)
        {
            // パスはフォルダか？
            if (Directory.Exists(path))
            {
                // フォルダの場合
                var files = GetX4Files(Directory.EnumerateFileSystemEntries(path), maxRecursion, currRecursion++);

                foreach (var file in files)
                {
                    yield return file;
                }
            }
            else
            {
                // ファイルの場合
                if (Path.GetExtension(path) == ".x4")
                {
                    yield return path;
                }
            }
        }

        yield break;
    }


    /// <summary>
    /// ドラッグ中
    /// </summary>
    /// <param name="dropInfo"></param>
    public void DragOver(IDropInfo dropInfo)
    {
        bool x4FileExists = GetX4Files(((DataObject)dropInfo.Data).GetFileDropList().OfType<string>(), 1).Any();

        dropInfo.Effects = x4FileExists ? DragDropEffects.Copy : DragDropEffects.None;
    }


    /// <summary>
    /// ドロップされた時
    /// </summary>
    /// <param name="dropInfo"></param>
    public void Drop(IDropInfo dropInfo)
    {
        _workAreaFileIO.OpenFiles(GetX4Files(((DataObject)dropInfo.Data).GetFileDropList().OfType<string>(), 1));
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
                RaisePropertyChanged(nameof(FileLoadingIsBusy));
                break;

            case nameof(_workAreaFileIO.Progress):
                RaisePropertyChanged(nameof(FileLoadingProgress));
                break;

            case nameof(_workAreaFileIO.LoadingFileName):
                RaisePropertyChanged(nameof(LoadingFileName));
                break;

            default:
                break;
        }
    }


    /// <summary>
    /// 新規作成
    /// </summary>
    private void CreateNew()
    {
        _workAreaFileIO.CreateNew();
        RaisePropertyChanged(nameof(ActiveContent));
    }


    /// <summary>
    /// 開く
    /// </summary>
    private void Open()
    {
        _workAreaFileIO.Open();
        RaisePropertyChanged(nameof(ActiveContent));
    }


    /// <summary>
    /// 問題を報告
    /// </summary>
    private void ReportIssue()
    {
        string url = ThisAssembly.Git.RepositoryUrl[..^4] + "/issues";
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }


    /// <summary>
    /// 更新確認ON/OFF
    /// </summary>
    private void SetCheckUpdateAtLaunch()
    {
        Configuration.Instance.CheckUpdateAtLaunch = !Configuration.Instance.CheckUpdateAtLaunch;
        CheckUpdateAtLaunch.Value = Configuration.Instance.CheckUpdateAtLaunch;
    }


    /// <summary>
    /// 更新を確認...
    /// </summary>
    private async Task CheckUpdate(bool isUserOperation = false)
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
    private void WindowLoaded()
    {
        try
        {
            // DB接続開始
            _mainWindowModel.Init();
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
    private void WindowClosing(CancelEventArgs e)
    {
        e.Cancel = _mainWindowModel.WindowClosing();
        if (!e.Cancel)
        {
            _empireOverviewWindow?.Close();

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
    private void OpenDBViewerWindow()
    {
        if (_dBViewerWindow is null)
        {
            _dBViewerWindow = new DBViewerWindow();
            _dBViewerWindow.Closed += (_, _) => { _dBViewerWindow = null; };
            _dBViewerWindow.Show();
        }

        _dBViewerWindow.Activate();
    }
}
