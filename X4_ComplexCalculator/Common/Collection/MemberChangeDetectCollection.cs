using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace X4_ComplexCalculator.Common.Collection
{
    /// <summary>
    /// コレクションの内容変更時のイベント(非同期)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public delegate Task NotifyCollectionChangedEventAsync(object sender, NotifyCollectionChangedEventArgs e);

    /// <summary>
    /// プロパティ変更時のイベント(非同期)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public delegate Task NotifyPropertyChangedEventAsync(object sender, PropertyChangedEventArgs e);


    /// <summary>
    /// メンバの変更もCollectionChangedとして検知するObservableCollection
    /// </summary>
    /// <typeparam name="T">INotifyPropertyChangedを実装したクラス</typeparam>
    public class MemberChangeDetectCollection<T> : SmartCollection<T> where T : INotifyPropertyChanged, IDisposable
    {
        #region メンバ
        /// <summary>
        /// 範囲追加時の追加開始位置
        /// </summary>
        private int AddRangeStart;
        #endregion


        /// <summary>
        /// コレクション変更時のイベント
        /// </summary>
        public event NotifyCollectionChangedEventAsync OnCollectionChangedAsync;


        /// <summary>
        /// コレクションのプロパティ変更時のイベント
        /// </summary>
        public event NotifyPropertyChangedEventAsync OnPropertyChangedAsync;


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
            // イベントハンドラーの設定/解除
            SetOrRemoveEventHandler(e);

            if (OnCollectionChangedAsync == null)
            {
                return;
            }

            Task.WhenAll(
                OnCollectionChangedAsync.GetInvocationList()
                                        .OfType<NotifyCollectionChangedEventAsync>()
                                        .Select(x => x.Invoke(sender, e))
            );
        }

        /// <summary>
        /// プロパティ変更時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var handler = OnPropertyChangedAsync;
            if (handler == null)
            {
                return;
            }

            await Task.WhenAll(
                handler.GetInvocationList()
                       .OfType<NotifyPropertyChangedEventAsync>()
                       .Select(async (x) => await x.Invoke(sender, e)));
        }


        /// <summary>
        /// イベントハンドラーの設定/解除
        /// </summary>
        /// <param name="e"></param>
        private void SetOrRemoveEventHandler(NotifyCollectionChangedEventArgs e)
        {
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
                    foreach (T item in Items.Skip(e.NewStartingIndex).Take(e.NewItems.Count))
                    {
                        item.PropertyChanged += OnPropertyChanged;
                    }
                    break;

                // 削除の場合
                case NotifyCollectionChangedAction.Remove:
                    foreach (T item in e.OldItems)
                    {
                        item.PropertyChanged -= OnPropertyChanged;
                        item.Dispose();
                    }
                    break;

                // リセットの場合
                case NotifyCollectionChangedAction.Reset:
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
            // 削除対象がなければ何もしない
            if (!range.Any())
            {
                return;
            }

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
                removeTarget.Dispose();
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
