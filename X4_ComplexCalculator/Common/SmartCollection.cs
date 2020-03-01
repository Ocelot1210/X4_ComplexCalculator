using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// ObservableCollectionの拡張(範囲追加メソッドをサポート)
    /// </summary>
    /// <typeparam name="T">任意のデータ型</typeparam>
    public class SmartCollection<T> : ObservableCollection<T>
    {
        #region コンストラクタ
        public SmartCollection() : base()
        {
        }

        public SmartCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public SmartCollection(List<T> list) : base(list)
        {
        }
        #endregion


        /// <summary>
        /// コレクションを追加
        /// </summary>
        /// <param name="range">追加するコレクション</param>
        public virtual void AddRange(IEnumerable<T> range)
        {
            CheckReentrancy();
            ((List<T>)Items).AddRange(range);

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }


        /// <summary>
        /// 一括削除
        /// </summary>
        /// <param name="range">削除対象</param>
        public void RemoveItems(IEnumerable<T> range)
        {
            CheckReentrancy();

            if (!(range is T[] removeTargets))
            {
                removeTargets = range.ToArray();
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
    }
}
