using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Threading;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Common.Localize;
using X4_ComplexCalculator.Main.Menu.Layout;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// 作業エリア管理用
/// </summary>
class WorkAreaManager : IDisposable
{
    #region メンバ
    /// <summary>
    /// ガベコレ用ストップウォッチ
    /// </summary>
    private readonly Stopwatch _gCStopWatch = new();


    /// <summary>
    ///ガベコレ用タイマー
    /// </summary>
    private readonly DispatcherTimer _gcTimer;


    /// <summary>
    /// レイアウト管理用クラス
    /// </summary>
    private readonly LayoutsManager _layoutsManager;
    #endregion


    #region プロパティ
    /// <summary>
    /// アクティブなワークスペース
    /// </summary>
    public WorkAreaViewModel? ActiveContent { set; get; }


    /// <summary>
    /// レイアウト一覧
    /// </summary>
    public ObservableCollection<LayoutMenuItem> Layouts => _layoutsManager.Layouts;


    /// <summary>
    /// 現在のレイアウトID
    /// </summary>
    public long ActiveLayoutID => _layoutsManager.ActiveLayout.Value?.LayoutID ?? -1;


    /// <summary>
    /// ワークエリア一覧
    /// </summary>
    public ObservableRangeCollection<WorkAreaViewModel> Documents { get; } = new();
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public WorkAreaManager()
    {
        _layoutsManager = new LayoutsManager(this);

        _gcTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, new EventHandler(GarvageCollect), Application.Current.Dispatcher);
        _gcTimer.Stop();

        // 現在のレイアウトが変更された場合、開いているドキュメントすべてに適用する
        _layoutsManager.ActiveLayout
            .Where(layout => layout is not null)
            .Select(layout => layout?.LayoutID ?? throw new InvalidOperationException())
            .Subscribe(layoutID =>
            {
                foreach (var document in Documents)
                {
                    document.SetLayout(layoutID);
                }
            });
    }


    /// <summary>
    /// 初期化
    /// </summary>
    public void Init() => _layoutsManager.Init();


    /// <summary>
    /// レイアウト保存
    /// </summary>
    public void SaveLayout() => _layoutsManager.SaveLayout(ActiveContent);


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
            var result = LocalizedMessageBox.Show("Lang:MainWindow_PlanClosingConfirmMessage", "Lang:Common_MessageBoxTitle_Confirmation", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel, vm.Title);

            switch (result)
            {
                // 保存する場合
                case MessageBoxResult.Yes:
                    vm.Save();
                    canceled = vm.HasChanged;       // 保存したはずなのに変更点がある(保存キャンセルされた等)場合、閉じないようにする
                    break;

                // 保存せずに閉じる場合
                case MessageBoxResult.No:
                    break;

                // キャンセルする場合
                case MessageBoxResult.Cancel:
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
            _gCStopWatch.Reset();

        }

        return canceled;
    }


    /// <summary>
    /// ガベコレ実行
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GarvageCollect(object? sender, EventArgs e)
    {
        // 最後のタブクローズから1000ミリ秒経過してからガベコレを発動する
        _gCStopWatch.Stop();
        if (1000 < _gCStopWatch.ElapsedMilliseconds)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // ガベコレ無効化
            _gcTimer.Stop();
        }
        _gCStopWatch.Start();
    }


    /// <inheritdoc />
    public void Dispose() => _layoutsManager.Dispose();
}
