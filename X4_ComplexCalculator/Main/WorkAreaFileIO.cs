using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// WorkAreaのファイル読み書き処理用クラス
/// </summary>
class WorkAreaFileIO : BindableBase
{
    #region メンバ
    /// <summary>
    /// ビジー状態か
    /// </summary>
    private bool _isBusy;


    /// <summary>
    /// 作業エリア管理用
    /// </summary>
    private readonly WorkAreaManager _workAreaManager;


    /// <summary>
    /// ファイル読み込み進捗
    /// </summary>
    private int _progress;


    /// <summary>
    /// 読込中のファイル名
    /// </summary>
    private string _loadingFileName = "";
    #endregion


    #region プロパティ
    /// <summary>
    /// ビジー状態か
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }


    /// <summary>
    /// ファイル読み込み進捗
    /// </summary>
    public int Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }


    /// <summary>
    /// 読込中のファイル名
    /// </summary>
    public string LoadingFileName
    {
        get => _loadingFileName;
        set => SetProperty(ref _loadingFileName, value);
    }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="workAreaManager">作業エリア管理用</param>
    public WorkAreaFileIO(WorkAreaManager workAreaManager)
    {
        _workAreaManager = workAreaManager;
    }


    /// <summary>
    /// 上書き保存
    /// </summary>
    public void Save() => _workAreaManager.ActiveContent?.Save();


    /// <summary>
    /// 名前を付けて保存
    /// </summary>
    public void SaveAs() => _workAreaManager.ActiveContent?.SaveAs();


    /// <summary>
    /// 新規作成
    /// </summary>
    public void CreateNew()
    {
        var vm = new WorkAreaViewModel(_workAreaManager.ActiveLayoutID);
        _workAreaManager.Documents.Add(vm);
        _workAreaManager.ActiveContent = vm;
    }


    /// <summary>
    /// 開く
    /// </summary>
    public void Open()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "X4: Complex calculator data file(*.x4)|*.x4|All Files|*.*",
            Multiselect = true
        };
        if (dlg.ShowDialog() == true)
        {
            OpenFiles(dlg.FileNames);
        }
    }


    /// <summary>
    /// ファイルを開く
    /// </summary>
    /// <param name="pathes">開く対象のファイルパス一覧</param>
    public void OpenFiles(IEnumerable<string> pathes)
    {
        if (!pathes.Any())
        {
            return;
        }

        try
        {
            var doevents = new DoEventsExecuter(0, 10);

            var prg = new ProgressEx<int>(0);
            var loaded = 0;
            var pathesCount = pathes.Count();
            var rate = 1.0 / pathesCount;

            prg.ProgressChanged += (sender, e) =>
            {
                Progress = (int)(e * rate + (loaded * rate * 100));
                doevents.DoEvents();
            };

            IsBusy = true;
            doevents.ForceDoEvents();
            var viewModels = new List<WorkAreaViewModel>(pathesCount);

            foreach (var path in pathes)
            {
                var vm = new WorkAreaViewModel(_workAreaManager.ActiveLayoutID);

                LoadingFileName = System.IO.Path.GetFileName(path);
                doevents.ForceDoEvents();

                vm.LoadFile(path, prg);
                viewModels.Add(vm);
                loaded++;
            }

            _workAreaManager.Documents.AddRange(viewModels);
        }
        catch (Exception e)
        {
            LocalizedMessageBox.Show("Lang:MainWindow_FaildToLoadFileMessage", "Lang:MainWindow_FaildToLoadFileMessageTitle", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message);
        }
        finally
        {
            IsBusy = false;
            Progress = 0;
        }
    }
}
