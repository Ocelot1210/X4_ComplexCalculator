using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Windows;
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
        #region コンストラクタ
        public ObservableRangeCollection() : base()
        {
            BindingOperations.EnableCollectionSynchronization(this, new object());
        }

        public ObservableRangeCollection(IEnumerable<T> collection) : base(collection)
        {
            BindingOperations.EnableCollectionSynchronization(this, new object());
        }

        public ObservableRangeCollection(List<T> list) : base(list)
        {
            BindingOperations.EnableCollectionSynchronization(this, new object());
        }
        #endregion

        
        /// <summary>
        /// コレクション変更時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // UIスレッドか？
            if (Application.Current.Dispatcher.Thread == Thread.CurrentThread)
            {
                // UIスレッドの場合、通常処理
                base.OnCollectionChanged(e);
            }
            else
            {
                // UIスレッドでない場合、Dispatcherで処理する
                Action<NotifyCollectionChangedEventArgs> action = OnCollectionChanged;
                Application.Current.Dispatcher.Invoke(action, e);
            }
        }
    }
}
