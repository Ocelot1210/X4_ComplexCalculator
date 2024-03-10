using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace X4_ComplexCalculator.Common.Collection;


/// <summary>
/// プロパティ変更時のイベント
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
public delegate void NotifyPropertyChangedEvent(object sender, PropertyChangedEventArgs e);


/// <summary>
/// メンバの変更もCollectionChangedとして検知するObservableCollection
/// </summary>
/// <typeparam name="INotifyPropertyChanged">INotifyPropertyChangedを実装したクラス</typeparam>
public class ObservablePropertyChangedCollection<INotifyPropertyChanged> : ObservableRangeCollection<INotifyPropertyChanged>
{
    #region イベント
    /// <summary>
    /// コレクションのプロパティ変更時のイベント
    /// </summary>
    public event NotifyPropertyChangedEvent? CollectionPropertyChanged;
    #endregion


    /// <summary>
    /// プロパティ変更時のイベント
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is null)
        {
            throw new InvalidOperationException();
        }

        CollectionPropertyChanged?.Invoke(sender, e);
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
                ArgumentNullException.ThrowIfNull(e.OldItems);
                ArgumentNullException.ThrowIfNull(e.NewItems);
                foreach (var item in e.OldItems.OfType<INotifyPropertyChanged>())
                {
                    WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.RemoveHandler(item, "PropertyChanged", OnPropertyChanged);
                }
                foreach (var item in e.NewItems.OfType<INotifyPropertyChanged>())
                {
                    WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(item, "PropertyChanged", OnPropertyChanged);
                }
                break;

            // 新規追加の場合
            case NotifyCollectionChangedAction.Add:
                ArgumentNullException.ThrowIfNull(e.NewItems);
                foreach (var item in Items.Skip(e.NewStartingIndex).Take(e.NewItems.Count).OfType<INotifyPropertyChanged>())
                {
                    WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(item, "PropertyChanged", OnPropertyChanged);
                }
                break;

            // 削除の場合
            case NotifyCollectionChangedAction.Remove:
                ArgumentNullException.ThrowIfNull(e.OldItems);
                foreach (var item in e.OldItems.OfType<INotifyPropertyChanged>())
                {
                    WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.RemoveHandler(item, "PropertyChanged", OnPropertyChanged);
                }
                break;

            // リセットの場合
            case NotifyCollectionChangedAction.Reset:
                foreach (var item in Items)
                {
                    WeakEventManager<INotifyPropertyChanged, PropertyChangedEventArgs>.AddHandler(item, "PropertyChanged", OnPropertyChanged);
                }
                break;

            // それ以外の場合
            default:
                break;
        }
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
