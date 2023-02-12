using AvalonDock;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Main;


/// <summary>
/// AvalonDock のドッキング/ドッキング解除時の親ウィンドウを子に設定するクラス
/// </summary>
internal class OwnerWindowSetter : Behavior<ContentControl>
{
    private DockingManager? _dockingManager;

    public DockingManager? DockingManager
    {
        get => _dockingManager;
        set
        {
            // 前回設定された DockingManager のイベント購読を解除する
            if (_dockingManager is not null)
            {
                _dockingManager.LayoutFloatingWindowControlCreated -= DockingManager_LayoutFloatingWindowControlCreated;
            }

            // 今回設定された DockingManager のイベントを購読する
            if (value is not null)
            {
                value.LayoutFloatingWindowControlCreated += DockingManager_LayoutFloatingWindowControlCreated;
            }

            _dockingManager = value;
        }
    }


    /// <summary>
    /// ドッキング解除時
    /// </summary>
    private void DockingManager_LayoutFloatingWindowControlCreated(object? sender, LayoutFloatingWindowControlCreatedEventArgs e)
    {
        e.LayoutFloatingWindowControl.Loaded += LayoutFloatingWindowControl_Loaded;
    }


    /// <summary>
    /// ドッキング解除時
    /// </summary>
    /// <remarks>
    /// <see cref="DockingManager.LayoutFloatingWindowControlCreated"/> では Window.GetWindow() が null を返す(ドッキング解除した Window がまだ作成されていない？)ため、
    /// <see cref="FrameworkElement.Loaded"/> を使用してドッキング解除を検知する
    /// </remarks>
    private void LayoutFloatingWindowControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DockingManager is null || sender is not FrameworkElement element)
        {
            return;
        }

        // このイベントは1度だけ呼ばれるようにしたいためイベント購読解除
        element.Loaded -= LayoutFloatingWindowControl_Loaded;

        // メッセージボックスの親ウィンドウを更新
        GetActiveViewModel()?.MessageBox.SetOwner(Window.GetWindow(element));

        // 親ウィンドウにドッキングした時のためにイベントを登録
        element.Unloaded += LayoutFloatingWindowControl_Unloaded;
    }


    /// <summary>
    /// ドッキング時
    /// </summary>
    /// <remarks>
    /// <see cref="DockingManager.LayoutFloatingWindowControlClosed"/> は何故か呼ばれない場合があるため、
    /// <see cref="FrameworkElement.Unloaded"/> を使用して親ウィンドウにドッキングした事を検知する
    /// </remarks>
    private void LayoutFloatingWindowControl_Unloaded(object sender, RoutedEventArgs e)
    {
        if (DockingManager is null || sender is not FrameworkElement element)
        {
            return;
        }

        // このイベントは1度だけ呼ばれるようにしたいためイベント購読解除
        element.Unloaded -= LayoutFloatingWindowControl_Unloaded;

        // メッセージボックスの親ウィンドウを更新
        GetActiveViewModel()?.MessageBox.SetOwner(Window.GetWindow(DockingManager));
    }


    /// <summary>
    /// アクティブな ViewModel を取得する
    /// </summary>
    /// <returns></returns>
    private WorkAreaViewModel? GetActiveViewModel()
    {
        return (Window.GetWindow(DockingManager).DataContext as Main.MainWindowViewModel)?.ActiveContent;
    }
}