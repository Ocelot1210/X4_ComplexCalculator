﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AvalonDock;
using GongSolutions.Wpf.DragDrop;
using Prism.Commands;
using Prism.Mvvm;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.Infrastructure;
using X4_ComplexCalculator.Main.Menu.File.Export;
using X4_ComplexCalculator.Main.Menu.File.Import;
using X4_ComplexCalculator.Main.Menu.File.Import.LoadoutImport;
using X4_ComplexCalculator.Main.Menu.File.Import.StationPlanImport;
using X4_ComplexCalculator.Main.Menu.Lang;
using X4_ComplexCalculator.Main.Menu.Layout;
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
        private readonly LanguagesManager _LangMgr = new LanguagesManager();

        /// <summary>
        /// 作業エリア管理用
        /// </summary>
        private readonly WorkAreaManager _WorkAreaManager = new WorkAreaManager();

        /// <summary>
        /// 作業エリアファイル読み書き用
        /// </summary>
        private readonly WorkAreaFileIO _WorkAreaFileIO;

        /// <summary>
        /// インポート/エクスポート処理用
        /// </summary>
        private readonly ImportExporter _ImportExporter;


        /// <summary>
        /// アップデート機能
        /// </summary>
        private readonly ApplicationUpdater _ApplicationUpdater = new ApplicationUpdater();
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
        /// DB更新
        /// </summary>
        public ICommand UpdateDBCommand { get; }


        /// <summary>
        /// 問題を報告
        /// </summary>
        public ICommand ReportIssueCommand { get; }


        /// <summary>
        /// 更新を確認...
        /// </summary>
        public ICommand CheckUpdateCommand { get; }


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
        public ObservableRangeCollection<LangMenuItem> Languages => _LangMgr.Languages;


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
            CheckUpdateCommand               = new DelegateCommand(() => CheckUpdate(isUserOperation: true));
            VersionInfoCommand               = new DelegateCommand(ShowVersionInfo);
            DocumentClosingCommand           = new DelegateCommand<DocumentClosingEventArgs>(DocumentClosing);
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
        private void Member_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
        /// 更新を確認...
        /// </summary>
        private async void CheckUpdate(bool isUserOperation = false)
        {
            if (_ApplicationUpdater.NowDownloading && isUserOperation)
            {
                LocalizedMessageBox.Show("Lang:CheckUpdateStartDownloadDescription",
                                         "Lang:CheckUpdate");
                return;
            }

            var latestVersion = await _ApplicationUpdater.CheckUpdate();
            if (latestVersion == null)
            {
                if (isUserOperation)
                {
                    LocalizedMessageBox.Show("Lang:CheckUpdateNoUpdateDescription",
                                             "Lang:CheckUpdate",
                                             param: new[] { VersionInfo.BaseVersion });
                }
                return;
            }

            var result = LocalizedMessageBox.Show("Lang:CheckUpdateHasUpdateDescription",
                                                  "Lang:CheckUpdate",
                                                  button: MessageBoxButton.YesNo,
                                                  param: new[] {
                                                      VersionInfo.BaseVersion,
                                                      latestVersion,
                                                  });
            if (result != MessageBoxResult.Yes) return;

            _ApplicationUpdater.StartDownloadByBackground();
            LocalizedMessageBox.Show("Lang:CheckUpdateStartDownloadDescription",
                                     "Lang:CheckUpdate");
        }


        /// <summary>
        /// バージョン情報
        /// </summary>
        private void ShowVersionInfo()
        {
            const string version = VersionInfo.DetailVersion;
            const string commit = ThisAssembly.Git.Sha;
            var dotnetVersion = Environment.Version.ToString();

            LocalizedMessageBox.Show("Lang:VersionInfoDescription", "Lang:VersionInfoTitle",
                                     icon: MessageBoxImage.Information,
                                     param: new[] { version, commit, dotnetVersion });
        }


        /// <summary>
        /// ウィンドウがロードされた時
        /// </summary>
        private void WindowLoaded()
        {
#if _DEBUG
            try
#endif
            {
                // DB接続開始
                _MainWindowModel.Init();
                _WorkAreaManager.Init();
                CheckUpdate();
            }
#if _DEBUG
            catch (Exception e)
            {
                LocalizedMessageBox.Show("Lang:UnexpectedErrorMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message, e.StackTrace ?? "");
                Environment.Exit(-1);
            }
#endif
        }


        /// <summary>
        /// ウィンドウが閉じられる時
        /// </summary>
        private void WindowClosing(CancelEventArgs e)
        {
            e.Cancel = _MainWindowModel.WindowClosing();
            if (!e.Cancel)
            {
                if (_ApplicationUpdater.FinishedDownload) _ApplicationUpdater.Update();
                else if (_ApplicationUpdater.NowDownloading)
                {
                    var dialog = new UpdateDownloadProglessDialog();
                    dialog.DataContext = new UpdateDownloadProgressViewModel(_ApplicationUpdater);
                    dialog.Show();
                }
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
    }
}
