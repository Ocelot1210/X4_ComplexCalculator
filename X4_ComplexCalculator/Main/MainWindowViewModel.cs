﻿using AvalonDock;
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
using X4_ComplexCalculator.Common.Localize;
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

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// メイン画面のViewModel
    /// </summary>
    class MainWindowViewModel : BindableBase, IDropTarget
    {
        #region メンバ
        /// <summary>
        /// メイン画面のModel
        /// </summary>
        private readonly MainWindowModel _MainWindowModel;


        /// <summary>
        /// 言語一覧管理用
        /// </summary>
        private readonly LanguagesManager _LangMgr = new();


        /// <summary>
        /// 作業エリア管理用
        /// </summary>
        private readonly WorkAreaManager _WorkAreaManager = new();


        /// <summary>
        /// 作業エリアファイル読み書き用
        /// </summary>
        private readonly WorkAreaFileIO _WorkAreaFileIO;


        /// <summary>
        /// インポート/エクスポート処理用
        /// </summary>
        private readonly ImportExporter _ImportExporter;


        /// <summary>
        /// 帝国の概要ウィンドウ
        /// </summary>
        private Window? _EmpireOverviewWindow;


        /// <summary>
        /// DBビュワーウィンドウ
        /// </summary>
        private Window? _DBViewerWindow;


        /// <summary>
        /// アップデート機能
        /// </summary>
        private readonly ApplicationUpdater _ApplicationUpdater = new();
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
        public ObservableCollection<WorkAreaViewModel> Documents => _WorkAreaManager.Documents;


        /// <summary>
        /// アクティブなワークスペース
        /// </summary>
        public WorkAreaViewModel? ActiveContent
        {
            set => _WorkAreaManager.ActiveContent = value;
            get => _WorkAreaManager.ActiveContent;
        }


        /// <summary>
        /// レイアウト一覧
        /// </summary>
        public ObservableCollection<LayoutMenuItem> Layouts => _WorkAreaManager.Layouts;


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
        public IReadOnlyList<LangMenuItem> Languages => _LangMgr.Languages;


        /// <summary>
        /// ファイル読み込みがビジー状態か
        /// </summary>
        public bool FileLoadingIsBusy => _WorkAreaFileIO.IsBusy;


        /// <summary>
        /// ファイル読み込み進捗
        /// </summary>
        public int FileLoadingProgress => _WorkAreaFileIO.Progress;


        /// <summary>
        /// 読込中のファイル名
        /// </summary>
        public string LoadingFileName => _WorkAreaFileIO.LoadingFileName;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            _WorkAreaFileIO                  = new WorkAreaFileIO(_WorkAreaManager);
            _MainWindowModel                 = new MainWindowModel(_WorkAreaManager, _WorkAreaFileIO);
            WindowLoadedCommand              = new DelegateCommand(WindowLoaded);
            WindowClosingCommand             = new DelegateCommand<CancelEventArgs>(WindowClosing);
            CreateNewCommand                 = new DelegateCommand(CreateNew);
            SaveLayout                       = new DelegateCommand(_WorkAreaManager.SaveLayout);
            SaveCommand                      = new DelegateCommand(_WorkAreaFileIO.Save);
            SaveAsCommand                    = new DelegateCommand(_WorkAreaFileIO.SaveAs);
            OpenCommand                      = new DelegateCommand(Open);
            UpdateDBCommand                  = new DelegateCommand(_MainWindowModel.UpdateDB);
            ReportIssueCommand               = new DelegateCommand(ReportIssue);
            CheckUpdateAtLaunch              = new ReactiveProperty<bool>(Configuration.Instance.CheckUpdateAtLaunch);
            SetCheckUpdateAtLaunchCommand    = new DelegateCommand(SetCheckUpdateAtLaunch);
            CheckUpdateCommand               = new AsyncReactiveCommand<bool>().WithSubscribe(CheckUpdate);
            VersionInfoCommand               = new DelegateCommand(ShowVersionInfo);
            DocumentClosingCommand           = new DelegateCommand<DocumentClosingEventArgs>(DocumentClosing);
            OpenEmpireOverviewWindowCommand  = new DelegateCommand(OpenEmpireOverviewWindow);
            OpenDBViewerWindowCommand        = new DelegateCommand(OpenDBViewerWindow);
            _WorkAreaFileIO.PropertyChanged += Member_PropertyChanged;

            _ImportExporter = new ImportExporter(_WorkAreaManager);
            Imports = new List<IImport>()
            {
                new StationCalculatorImport(new DelegateCommand<IImport>(_ImportExporter.Import)),
                new StationPlanImport(new DelegateCommand<IImport>(_ImportExporter.Import)),
                new LoadoutImport(new DelegateCommand<IImport>(_ImportExporter.Import)),
                //new SaveDataImport(new DelegateCommand<IImport>(_Model.Import))   // 作成中のため未リリース
            };

            Exports = new List<IExport>()
            {
                new StationCalculatorExport(new DelegateCommand<IExport>(_ImportExporter.Export))
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
            _WorkAreaFileIO.OpenFiles(GetX4Files(((DataObject)dropInfo.Data).GetFileDropList().OfType<string>(), 1));
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
                case nameof(_WorkAreaFileIO.IsBusy):
                    RaisePropertyChanged(nameof(FileLoadingIsBusy));
                    break;

                case nameof(_WorkAreaFileIO.Progress):
                    RaisePropertyChanged(nameof(FileLoadingProgress));
                    break;

                case nameof(_WorkAreaFileIO.LoadingFileName):
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
            _WorkAreaFileIO.CreateNew();
            RaisePropertyChanged(nameof(ActiveContent));
        }


        /// <summary>
        /// 開く
        /// </summary>
        private void Open()
        {
            _WorkAreaFileIO.Open();
            RaisePropertyChanged(nameof(ActiveContent));
        }


        /// <summary>
        /// 問題を報告
        /// </summary>
        private void ReportIssue()
        {
            const string url = ThisAssembly.Git.RepositoryUrl + "/issues";
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
            if (_ApplicationUpdater.FinishedDownload && isUserOperation)
            {
                LocalizedMessageBox.Show("Lang:CheckUpdate_FinishedDownloadDescription",
                                         "Lang:CheckUpdate_Title", icon: MessageBoxImage.Information);
                return;
            }
            else if (_ApplicationUpdater.NowDownloading && isUserOperation)
            {
                LocalizedMessageBox.Show("Lang:CheckUpdate_StartDownloadDescription",
                                         "Lang:CheckUpdate_Title", icon: MessageBoxImage.Information);
                return;
            }

            string? latestVersion;
            try
            {
                latestVersion = await _ApplicationUpdater.CheckUpdate();
            }
            catch (HttpRequestException)
            {
                if (isUserOperation)
                {
                    LocalizedMessageBox.Show("Lang:CheckUpdate_FailedDescription",
                                             "Lang:CheckUpdate_Title", icon: MessageBoxImage.Error);
                }
                return;
            }
            if (latestVersion is null)
            {
                if (isUserOperation)
                {
                    LocalizedMessageBox.Show("Lang:CheckUpdate_NoUpdateDescription",
                                             "Lang:CheckUpdate_Title", icon: MessageBoxImage.Information,
                                             param: new[] { VersionInfo.BaseVersion });
                }
                return;
            }

            var result = LocalizedMessageBox.Show("Lang:CheckUpdate_HasUpdateDescription",
                                                  "Lang:CheckUpdate_Title",
                                                  button: MessageBoxButton.YesNo,
                                                  icon: MessageBoxImage.Question,
                                                  param: new[] {
                                                      VersionInfo.BaseVersion,
                                                      latestVersion,
                                                  });
            if (result != MessageBoxResult.Yes) return;

            _ApplicationUpdater.StartDownloadByBackground();
            LocalizedMessageBox.Show("Lang:CheckUpdate_StartDownloadDescription",
                                     "Lang:CheckUpdate_Title", icon: MessageBoxImage.Information);
        }


        /// <summary>
        /// バージョン情報
        /// </summary>
        private void ShowVersionInfo()
        {
            const string version = VersionInfo.DetailVersion;
            const string commit = ThisAssembly.Git.Sha;
            const string date = ThisAssembly.Git.CommitDate;
            var dotnetVersion = Environment.Version.ToString();

            LocalizedMessageBox.Show("Lang:MainWindow_Menu_Help_VersionInfo_MessageDescription", "Lang:MainWindow_Menu_Help_VersionInfo_MessageTitle",
                                     icon: MessageBoxImage.Information,
                                     param: new[] { version, commit, date, dotnetVersion });
        }


        /// <summary>
        /// ウィンドウがロードされた時
        /// </summary>
        private void WindowLoaded()
        {
            try
            {
                // DB接続開始
                _MainWindowModel.Init();
                _WorkAreaManager.Init();

                // 更新チェックが有効な場合のみ更新を確認する
                if (Configuration.Instance.CheckUpdateAtLaunch)
                {
                    CheckUpdateCommand.Execute(false);
                }
            }
            catch (Exception e)
            {
                LocalizedMessageBox.Show("Lang:MainWindow_UnexpectedErrorMessage", "Lang:Common_MessageBoxTitle_Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message, e.StackTrace ?? "");
                Environment.Exit(-1);
            }
        }


        /// <summary>
        /// ウィンドウが閉じられる時
        /// </summary>
        private void WindowClosing(CancelEventArgs e)
        {
            e.Cancel = _MainWindowModel.WindowClosing();
            if (!e.Cancel)
            {
                _EmpireOverviewWindow?.Close();

                if (_ApplicationUpdater.FinishedDownload) _ApplicationUpdater.Update();
                else if (_ApplicationUpdater.NowDownloading)
                {
                    var dialog = new UpdateDownloadProglessDialog();
                    dialog.DataContext = new UpdateDownloadProgressViewModel(_ApplicationUpdater);
                    dialog.Show();
                }
                _WorkAreaManager.Dispose();
            }
        }

        /// <summary>
        /// タブが閉じられる時
        /// </summary>
        /// <param name="e"></param>
        private void DocumentClosing(DocumentClosingEventArgs e)
        {
            if (e.Document.Content is WorkAreaViewModel WorkArea)
            {
                e.Cancel = _WorkAreaManager.DocumentClosing(WorkArea);
            }
        }


        /// <summary>
        /// 帝国の概要ウィンドウを開く
        /// </summary>
        private void OpenEmpireOverviewWindow()
        {
            if (_EmpireOverviewWindow is null)
            {
                _EmpireOverviewWindow = new EmpireOverviewWindow(Documents);
                _EmpireOverviewWindow.Closed += (obj, e) => { _EmpireOverviewWindow = null; };
                _EmpireOverviewWindow.Show();
            }

            _EmpireOverviewWindow.Activate();
        }


        /// <summary>
        /// DBビュワーウィンドウを開く
        /// </summary>
        private void OpenDBViewerWindow()
        {
            if (_DBViewerWindow is null)
            {
                _DBViewerWindow = new DBViewerWindow();
                _DBViewerWindow.Closed += (_, _) => { _DBViewerWindow = null; };
                _DBViewerWindow.Show();
            }

            _DBViewerWindow.Activate();
        }
    }
}
