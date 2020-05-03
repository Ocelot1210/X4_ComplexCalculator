using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Data;
using System.Windows.Threading;

namespace X4_ComplexCalculator.Common.Collection
{
    /// <summary>
    /// ObservableCollectionの拡張(範囲操作とマルチスレッドをサポート)
    /// </summary>
    /// <typeparam name="T">任意のデータ型</typeparam>
    public class ObservableRangeCollection<T> : WpfObservableRangeCollection<T>
    {
        #region メンバ
        /// <summary>
        /// CollectionChangedイベント時に使用
        /// </summary>
        private Dispatcher Dispatcher { get; }

        /// <summary>
        /// イベントが無効化されているか
        /// </summary>
        protected bool EventDisabled = false;
        #endregion

        #region コンストラクタ
        public ObservableRangeCollection() : base()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        public ObservableRangeCollection(IEnumerable<T> collection) : base(collection)
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        public ObservableRangeCollection(List<T> list) : base(list)
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }
        #endregion

        
        /// <summary>
        /// コレクション変更時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // UIスレッドか？
            if (Dispatcher == null || Dispatcher.Thread == Thread.CurrentThread)
            {
                // UIスレッドの場合、通常処理
                base.OnCollectionChanged(e);
            }
            else
            {
                // UIスレッドでない場合、Dispatcherで処理する
                Action<NotifyCollectionChangedEventArgs> action = OnCollectionChanged;
                Dispatcher.Invoke(action, e);
            }
        }
    }
}
