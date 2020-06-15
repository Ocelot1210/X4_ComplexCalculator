using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace X4_ComplexCalculator.Common
{
    public class DoEventsExecuter
    {
        /// <summary>
        /// N回に1回実行するか
        /// </summary>
        readonly int _ExecuteNum;

        /// <summary>
        /// 最低でも実行する間隔
        /// </summary>
        readonly long _ExecuteMs;

        /// <summary>
        /// 実行間隔計測用ストップウォッチ
        /// </summary>
        readonly Stopwatch _Stopwatch = new Stopwatch();

        /// <summary>
        /// DoEventsが呼ばれた回数
        /// </summary>
        int CallCount;

        /// <summary>
        /// コールバック処理
        /// </summary>
        DispatcherOperationCallback _DispatcherOperationCallback = new DispatcherOperationCallback(obj => { ((DispatcherFrame)obj).Continue = false; return null; });

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="executeNum">N回に1回実行するか(-1で判定無効化)</param>
        /// <param name="executeMs">最低でも実行する間隔(-1で判定無効化)</param>
        public DoEventsExecuter(int executeNum, long executeMs)
        {
            if (executeNum == -1 && executeMs == -1)
            {
                throw new ArgumentException();
            }
            _ExecuteNum = executeNum;
            _ExecuteMs = executeMs;
            _Stopwatch.Start();
        }


        /// <summary>
        /// DoEvents実行
        /// </summary>
        public void DoEvents()
        {
            if ((_ExecuteNum != -1 && _ExecuteNum < CallCount++) || (_ExecuteMs != -1 && _ExecuteMs < _Stopwatch.ElapsedMilliseconds))
            {
                DoEventsMain();
                CallCount = 0;
                _Stopwatch.Reset();
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
                CallCount = 0;
                _Stopwatch.Restart();
            }
        }


        /// <summary>
        /// DoEvents実行メイン
        /// </summary>
        private void DoEventsMain()
        {
            var frame = new DispatcherFrame();
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, _DispatcherOperationCallback, frame);
            Dispatcher.PushFrame(frame);
        }
    }
}
