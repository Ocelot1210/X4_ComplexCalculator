using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
    /// プロパティ変更時のイベント
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void NotifyPropertyChangedEvent(object sender, PropertyChangedEventArgs e);


    /// <summary>
    /// メンバの変更もCollectionChangedとして検知するObservableCollection
    /// </summary>
    /// <typeparam name="T">INotifyPropertyChangedを実装したクラス</typeparam>
    public class ObservablePropertyChangedCollection<INotifyPropertyChanged> : ObservableRangeCollection<INotifyPropertyChanged>
    {
        #region イベント
        /// <summary>
        /// コレクション変更時のイベント
        /// </summary>
        public event NotifyCollectionChangedEventAsync CollectionChangedAsync;


        /// <summary>
        /// コレクションのプロパティ変更時のイベント(非同期)
        /// </summary>
        public event NotifyPropertyChangedEventAsync CollectionPropertyChangedAsync;


        /// <summary>
        /// コレクションのプロパティ変更時のイベント
        /// </summary>
        public event NotifyPropertyChangedEvent CollectionPropertyChanged;
        #endregion

        #region コンストラクタ
        public ObservablePropertyChangedCollection() : base()
        {
            CollectionChanged += CollectionChangedEvent;
        }

        public ObservablePropertyChangedCollection(IEnumerable<INotifyPropertyChanged> collection) : base(collection)
        {
            CollectionChanged += CollectionChangedEvent;
        }

        public ObservablePropertyChangedCollection(List<INotifyPropertyChanged> list) : base(list)
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
            InitEventHandler(e);

            // イベントが無効化されていれば何もしない
            if (EventDisabled)
            {
                return;
            }

            // 非同期版イベントが購読されていなければ何もしない
            if (CollectionChangedAsync == null)
            {
                return;
            }

            
            Task.WhenAll(
                CollectionChangedAsync.GetInvocationList()
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
            // イベントが無効化されていれば何もしない
            if (EventDisabled)
            {
                return;
            }

            CollectionPropertyChanged?.Invoke(sender, e);

            // 非同期版イベントが購読されていなければ何もしない
            var handler = CollectionPropertyChangedAsync;
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
        private void InitEventHandler(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                // 置換の場合
                case NotifyCollectionChangedAction.Replace:
                    foreach (INotifyPropertyChanged item in e.OldItems)
                    {
                        WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.RemoveHandler(item, "PropertyChanged", OnPropertyChanged);
                    }
                    foreach (INotifyPropertyChanged item in e.NewItems)
                    {
                        WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(item, "PropertyChanged", OnPropertyChanged);
                    }
                    break;

                // 新規追加の場合
                case NotifyCollectionChangedAction.Add:
                    foreach (INotifyPropertyChanged item in Items.Skip(e.NewStartingIndex).Take(e.NewItems.Count))
                    {
                        WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(item, "PropertyChanged", OnPropertyChanged);
                    }
                    break;

                // 削除の場合
                case NotifyCollectionChangedAction.Remove:
                    foreach (INotifyPropertyChanged item in e.OldItems)
                    {
                        WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.RemoveHandler(item, "PropertyChanged", OnPropertyChanged);
                    }
                    break;

                // それ以外の場合
                default:
                    break;
            }
        }



        /// <summary>
        /// クリアしてコレクションを追加
        /// </summary>
        /// <param name="range">追加するコレクション</param>
        public override void Reset(IEnumerable<INotifyPropertyChanged> range)
        {
            base.Reset(range);
        }


        /// <summary>
        /// コレクション変更時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            InitEventHandler(e);
            base.OnCollectionChanged(e);
        }
    }
}
