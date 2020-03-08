using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// ObservableCollectionの拡張(範囲追加メソッドをサポート)
    /// </summary>
    /// <typeparam name="T">任意のデータ型</typeparam>
    public class SmartCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// CollectionChangedイベント時に使用
        /// </summary>
        public Dispatcher Dispatcher { get; set; }


        #region コンストラクタ
        public SmartCollection() : base()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        public SmartCollection(IEnumerable<T> collection) : base(collection)
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        public SmartCollection(List<T> list) : base(list)
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }
        #endregion


        /// <summary>
        /// コレクションを追加
        /// </summary>
        /// <param name="range">追加するコレクション</param>
        public virtual void AddRange(IEnumerable<T> range)
        {
            CheckReentrancy();

            foreach(var itm in range)
            {
                Items.Add(itm);
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        /// <summary>
        /// 一括削除
        /// </summary>
        /// <param name="range">削除対象</param>
        public virtual void RemoveItems(IEnumerable<T> range)
        {
            CheckReentrancy();

            if (!(range is T[] removeTargets))
            {
                removeTargets = range.ToArray();
            }

            // 削除対象が1つだけならRemoveを使用
            if (removeTargets.Length == 1)
            {
                Remove(removeTargets[0]);
                return;
            }

            foreach(var removeTarget in removeTargets)
            {
                Items.Remove(removeTarget);
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        /// <summary>
        /// クリアしてコレクションを追加
        /// </summary>
        /// <param name="range">追加するコレクション</param>
        public void Reset(IEnumerable<T> range)
        {
            Items.Clear();

            // 内容がある場合のみ処理
            AddRange(range);
        }

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

        /// <summary>
        /// 要素を置換する
        /// </summary>
        /// <param name="oldItem">古い要素</param>
        /// <param name="newItem">新しい要素</param>
        public void Replace(T oldItem, T newItem)
        {
            var idx = Items.IndexOf(oldItem);
            Items.RemoveAt(idx);
            Items.Insert(idx, newItem);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem));
        }
    }
}
