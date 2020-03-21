using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace X4_ComplexCalculator.Common.Collection
{
    /// <summary>
    /// ObservableCollectionの拡張(範囲操作とマルチスレッドをサポート)
    /// </summary>
    /// <typeparam name="T">任意のデータ型</typeparam>
    public class SmartCollection<T> : ObservableCollection<T>
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

            EventDisabled = true;
            foreach(var itm in range)
            {
                Add(itm);
            }
            EventDisabled = false;

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

            // 削除対象要素番号(順番を維持するため要素番号の降順にする)
            var indexies = range.Select(x => Items.IndexOf(x))
                                .Where(x => -1 < x)
                                .OrderByDescending(x => x);

            EventDisabled = true;
            foreach (var idx in indexies)
            {
                RemoveAt(idx);
            }
            EventDisabled = false;

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        /// <summary>
        /// クリアしてコレクションを追加
        /// </summary>
        /// <param name="range">追加するコレクション</param>
        public virtual void Reset(IEnumerable<T> range)
        {
            Items.Clear();
            AddRange(range);
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



        /// <summary>
        /// プロパティ変更時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            // イベントが無効化されていれば何もしない
            if (EventDisabled)
            {
                return;
            }

            base.OnPropertyChanged(e);
        }


        /// <summary>
        /// コレクション変更時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // イベントが無効化されていれば何もしない
            if (EventDisabled)
            {
                return;
            }

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
