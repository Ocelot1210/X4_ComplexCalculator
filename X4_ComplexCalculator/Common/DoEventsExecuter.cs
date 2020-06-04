﻿using System.Diagnostics;
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
        /// コンストラクタ
        /// </summary>
        /// <param name="executeNum">N回に1回実行するか</param>
        /// <param name="executeMs">最低でも実行する間隔</param>
        public DoEventsExecuter(int executeNum, long executeMs)
        {
            _ExecuteNum = executeNum;
            _ExecuteMs = executeMs;
            _Stopwatch.Start();
        }


        /// <summary>
        /// DoEvents実行
        /// </summary>
        public void DoEvents()
        {
            if (_ExecuteNum < CallCount++ || _ExecuteMs < _Stopwatch.ElapsedMilliseconds)
            {
                DoEventsMain();
                CallCount = 0;
                _Stopwatch.Reset();
            }
        }


        /// <summary>
        /// DoEvents実行メイン
        /// </summary>
        private void DoEventsMain()
        {
            var frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(obj =>
            {
                ((DispatcherFrame)obj).Continue = false;
                return null;
            });
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);
        }
    }
}
