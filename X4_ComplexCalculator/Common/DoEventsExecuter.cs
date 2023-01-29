using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace X4_ComplexCalculator.Common;

public class DoEventsExecuter
{
    /// <summary>
    /// N回に1回実行するか
    /// </summary>
    readonly int _executeNum;

    /// <summary>
    /// 最低でも実行する間隔
    /// </summary>
    readonly long _executeMs;

    /// <summary>
    /// 実行間隔計測用ストップウォッチ
    /// </summary>
    readonly Stopwatch _stopwatch = new();

    /// <summary>
    /// DoEventsが呼ばれた回数
    /// </summary>
    int _callCount;

    /// <summary>
    /// コールバック処理
    /// </summary>
    readonly DispatcherOperationCallback _dispatcherOperationCallback
        = new(obj => { ((DispatcherFrame)obj).Continue = false; return null; });

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="executeNum">N回に1回実行するか(-1で判定無効化)</param>
    /// <param name="executeMs">最低でも実行する間隔(-1で判定無効化)</param>
    public DoEventsExecuter(int executeNum, long executeMs)
    {
        if (executeNum == -1 && executeMs == -1)
        {
            throw new ArgumentException("Invalid parameter", nameof(executeNum));
        }
        _executeNum = executeNum;
        _executeMs = executeMs;
        _stopwatch.Start();
    }


    /// <summary>
    /// DoEvents実行
    /// </summary>
    public void DoEvents()
    {
        if ((_executeNum != -1 && _executeNum < _callCount++) || (_executeMs != -1 && _executeMs < _stopwatch.ElapsedMilliseconds))
        {
            DoEventsMain();
            _callCount = 0;
            _stopwatch.Reset();
        }
    }

    /// <summary>
    /// DoEventsを強制的に発生させる
    /// </summary>
    /// <param name="resetTimer">タイマーをリセットするか</param>
    public void ForceDoEvents(bool resetTimer = false)
    {
        DoEventsMain();
        if (resetTimer)
        {
            _callCount = 0;
            _stopwatch.Restart();
        }
    }


    /// <summary>
    /// DoEvents実行メイン
    /// </summary>
    private void DoEventsMain()
    {
        var frame = new DispatcherFrame();
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, _dispatcherOperationCallback, frame);
        Dispatcher.PushFrame(frame);
    }
}
