using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace X4_ComplexCalculator.Common
{
    public delegate Task NotifyCollectionChangedEventAsync(object sender, NotifyCollectionChangedEventArgs e);

    /// <summary>
    /// メンバの変更もCollectionChangedとして検知するObservableCollection
    /// </summary>
    /// <typeparam name="T">INotifyPropertyChangedを実装したクラス</typeparam>
    public class MemberChangeDetectCollection<T> : SmartCollection<T> where T : INotifyPropertyChanged
    {
        #region メンバ
        /// <summary>
        /// 範囲追加時の追加開始位置
        /// </summary>
        private int AddRangeStart;
        #endregion


        /// <summary>
        /// コレクションとプロパティ変更時のイベント
        /// </summary>
        public event NotifyCollectionChangedEventAsync OnCollectionOrPropertyChanged;



        #region コンストラクタ
        public MemberChangeDetectCollection() : base()
        {
            CollectionChanged += CollectionChangedEvent;
        }

        public MemberChangeDetectCollection(IEnumerable<T> collection) : base(collection)
        {
            CollectionChanged += CollectionChangedEvent;
        }

        public MemberChangeDetectCollection(List<T> list) : base(list)
        {
            CollectionChanged += CollectionChangedEvent;
        }
        #endregion


        /// <summary>
        /// コレクション変更時のイベントハンドラー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectionChangedEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            async void OnPropertyChanged(object obj, PropertyChangedEventArgs ev)
            {
                var handler = OnCollectionOrPropertyChanged;
                if (handler == null)
                {
                    return;
                }

                await Task.WhenAll(
                    handler.GetInvocationList()
                           .OfType<NotifyCollectionChangedEventAsync>()
                           .Select(async (x) => await x.Invoke(this, null)));
            }

            switch (e.Action)
            {
                // 置換の場合
                case NotifyCollectionChangedAction.Replace:
                    foreach (T item in e.OldItems)
                    {
                        item.PropertyChanged -= OnPropertyChanged;
                    }
                    foreach (T item in e.NewItems)
                    {
                        item.PropertyChanged += OnPropertyChanged;
                    }
                    break;

                // 新規追加の場合
                case NotifyCollectionChangedAction.Add:
                    foreach (T item in e.NewItems)
                    {
                        item.PropertyChanged += OnPropertyChanged;
                    }
                    break;

                // 削除の場合
                case NotifyCollectionChangedAction.Remove:
                    foreach (T item in e.OldItems)
                    {
                        item.PropertyChanged -= OnPropertyChanged;
                        if (item is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                    break;

                // リセットの場合
                case NotifyCollectionChangedAction.Reset:
                    // AddRangeされたか?
                    if (AddRangeStart != -1)
                    {
                        foreach (T item in Items.Skip(AddRangeStart))
                        {
                            item.PropertyChanged += OnPropertyChanged;
                        }
                    }

                    break;

                // それ以外の場合
                default:
                    break;
            }

            //OnCollectionAndPropertyChanged?.Invoke(this, e);
            OnPropertyChanged(sender, new PropertyChangedEventArgs(""));
        }


        /// <summary>
        /// コレクションを追加
        /// </summary>
        /// <param name="range">追加するコレクション</param>
        public override void AddRange(IEnumerable<T> range)
        {
            // 追加対象が無ければ何もしない
            if (!range.Any())
            {
                return;
            }

            // 追加対象が1つだけならAddを使う
            if (!range.Skip(1).Any())
            {
                Add(range.First());
                return;
            }

            AddRangeStart = Items.Count;
            (Items as List<T>).AddRange(range);

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            AddRangeStart = -1;
        }

        /// <summary>
        /// 一括削除
        /// </summary>
        /// <param name="range">削除対象</param>
        public override void RemoveItems(IEnumerable<T> range)
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

            foreach (var removeTarget in removeTargets)
            {
                Items.Remove(removeTarget);
                if (removeTarget is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
