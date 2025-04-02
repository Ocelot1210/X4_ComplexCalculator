using Collections.Pooled;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;
using X4_ComplexCalculator.Main.Menu.Layout;
using X4_ComplexCalculator.Main.WorkArea;
using X4_ComplexCalculator.Main.WorkArea.SaveDataReaders;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// 作業エリア管理用
/// </summary>
partial class WorkAreaManager : ObservableRecipientEx
{
    #region メンバ
    /// <summary>
    /// ガベコレ用ストップウォッチ
    /// </summary>
    private readonly Stopwatch _gcStopWatch = new();


    /// <summary>
    /// ガベコレ用タイマー
    /// </summary>
    private readonly DispatcherTimer _gcTimer;


    /// <summary>
    /// レイアウト管理用クラス
    /// </summary>
    private readonly LayoutsManager _layoutsManager;


    /// <summary>
    /// メッセージボックス表示用
    /// </summary>
    private readonly ILocalizedMessageBox _localizedMessageBox;


    /// <summary>
    /// 保存ファイル読み込み時の進捗表示用
    /// </summary>
    private readonly SaveDataReaderProgress _saveDataReaderProgress;
    #endregion


    #region プロパティ
    /// <summary>
    /// アクティブなワークスペース
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial WorkAreaViewModel? ActiveContent { set; get; }


    /// <summary>
    /// レイアウト一覧
    /// </summary>
    public ObservableCollection<LayoutMenuItem> Layouts => _layoutsManager.Layouts;


    /// <summary>
    /// 現在のレイアウトID
    /// </summary>
    public long ActiveLayoutID => _layoutsManager.ActiveLayout?.LayoutID ?? -1;


    /// <summary>
    /// ワークエリア一覧
    /// </summary>
    public ObservableRangeCollection<WorkAreaViewModel> Documents { get; } = new();
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messageBox">メッセージボックス表示用</param>
    public WorkAreaManager(IMessenger messenger, ILocalizedMessageBox messageBox, SaveDataReaderProgress saveDataReaderProgress) : base(messenger)
    {
        _localizedMessageBox = messageBox;
        _saveDataReaderProgress = saveDataReaderProgress;

        _layoutsManager = new LayoutsManager(messenger, _localizedMessageBox);

        _gcTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, new EventHandler(GarvageCollect), Application.Current.Dispatcher);
        _gcTimer.Stop();

        Messenger.RegisterPropertyChangedMessage(this, static (LayoutsManager x) => x.ActiveLayout, static (r, m) => r.OnActiveLayoutChanged(m));
        Messenger.RegisterRequestMessage(this, static (r) => r.ActiveContent);

        IsActive = true;
    }


    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        _layoutsManager.Init();
    }


    /// <summary>
    /// レイアウト保存
    /// </summary>
    public void SaveLayout()
    {
        _layoutsManager.SaveLayout(ActiveContent);
    }


    /// <summary>
    /// 新規作成
    /// </summary>
    public void CreateNewDocument()
    {
        var messenger = new WeakReferenceMessenger();
        var vm = new WorkAreaViewModel(messenger, ActiveLayoutID, _localizedMessageBox.Clone());
        Documents.Add(vm);
        ActiveContent = vm;
    }


    /// <summary>
    /// 保存
    /// </summary>
    public void SaveDocument()
    {
        ActiveContent?.Save();
    }


    /// <summary>
    /// 名前を付けて保存
    /// </summary>
    public void SaveAsDocument()
    {
        ActiveContent?.SaveAs();
    }


    /// <summary>
    /// 開く
    /// </summary>
    public async Task OpenAsync()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "X4: Complex calculator data file(*.x4)|*.x4|All Files|*.*",
            Multiselect = true
        };
        if (dlg.ShowDialog() == true)
        {
            await OpenFilesAsync(dlg.FileNames);
        }
    }


    /// <summary>
    /// ファイルを開く
    /// </summary>
    /// <param name="paths">開く対象のファイルパス一覧</param>
    public async Task OpenFilesAsync(IEnumerable<string> paths)
    {
        using var pathsList = new PooledList<string>(paths);

        if (!pathsList.Any())
        {
            return;
        }

        try
        {
            var prg = new ProgressEx<int>(0);
            var loaded = 0;
            var rate = 1.0 / pathsList.Count;

            prg.ProgressChanged += (sender, e) =>
            {
                _saveDataReaderProgress.Progress = (int)(e * rate + (loaded * rate * 100));
            };

            _saveDataReaderProgress.IsBusy = true;
            using var viewModels = new PooledList<WorkAreaViewModel>(
                Enumerable.Range(0, pathsList.Count).Select(x => new WorkAreaViewModel(new WeakReferenceMessenger(), ActiveLayoutID, _localizedMessageBox.Clone()))
                );

            await Task.Run(() =>
            {
                foreach (var (vm, path) in viewModels.Zip(paths))
                {
                    _saveDataReaderProgress.LoadingFileName = System.IO.Path.GetFileName(path);
                    vm.LoadFile(path, prg);
                    loaded++;
                }
            });

            Documents.AddRange(viewModels);
        }
        catch (Exception e)
        {
            _localizedMessageBox.Error("Lang:MainWindow_FaildToLoadFileMessage", "Lang:MainWindow_FaildToLoadFileMessageTitle", e.Message);
        }
        finally
        {
            _saveDataReaderProgress.IsBusy = false;
            _saveDataReaderProgress.Progress = 0;
        }
    }


    /// <summary>
    /// 作業エリアが閉じられる時
    /// </summary>
    /// <param name="vm">閉じようとしている作業エリア</param>
    /// <returns></returns>
    public bool DocumentClosing(WorkAreaViewModel vm)
    {
        var canceled = false;

        // 変更があったか？
        if (vm.HasChanged)
        {
            (string, string?)[] buttons = {
                ("Lang:MainWindow_PlanClosingConfirmMessage_Save", null),
                ("Lang:MainWindow_PlanClosingConfirmMessage_DontSave", "Lang:MainWindow_PlanClosingConfirmMessage_DontSave_Description"),
                ("Lang:MainWindow_PlanClosingConfirmMessage_Cancel", "Lang:MainWindow_PlanClosingConfirmMessage_Cancel_Description"),
            };
            var result = vm.MessageBox.MultiChoiceInfo("Lang:MainWindow_PlanClosingConfirmMessage", "Lang:Common_MessageBoxTitle_Confirmation", buttons, 2, vm.Title);

            switch (result)
            {
                // 保存する場合
                case 0:
                    vm.Save();
                    canceled = vm.HasChanged;       // 保存したはずなのに変更点がある(保存キャンセルされた等)場合、閉じないようにする
                    break;

                // 保存せずに閉じる場合
                case 1:
                    break;

                // キャンセルする場合
                default:
                    canceled = true;
                    break;
            }
        }

        // 閉じる場合、リソースを開放
        if (!canceled)
        {
            vm.Dispose();
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                Documents.Remove(vm);

                // 最後のタブを閉じた時にAvalonDockのActiveContentが更新されないためここでnullにする
                // → nullにしないと閉じたはずのタブを保存できてしまう
                if (Documents.Count == 0)
                {
                    ActiveContent = null;
                }

            }), DispatcherPriority.Background);

            // ガベコレ用タイマー開始
            if (!_gcTimer.IsEnabled)
            {
                _gcTimer.Start();
            }

            // 時間計測用ストップウォッチを初期化
            _gcStopWatch.Reset();

        }

        return canceled;
    }


    /// <summary>
    /// 現在のレイアウトが変更された時
    /// </summary>
    private void OnActiveLayoutChanged(PropertyChangedMessage<LayoutMenuItem?> message)
    {
        // 現在のレイアウトを開いているドキュメントすべてに適用する
        if (message.NewValue is not null)
        {
            foreach (var document in Documents)
            {
                document.LayoutManager.SetLayout(message.NewValue.LayoutID);
            }
        }
    }


    /// <summary>
    /// ガベコレ実行
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GarvageCollect(object? sender, EventArgs e)
    {
        // 最後のタブクローズから1000ミリ秒経過してからガベコレを発動する
        _gcStopWatch.Stop();
        if (1000 < _gcStopWatch.ElapsedMilliseconds)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // ガベコレ無効化
            _gcTimer.Stop();
        }
        _gcStopWatch.Start();
    }
}
