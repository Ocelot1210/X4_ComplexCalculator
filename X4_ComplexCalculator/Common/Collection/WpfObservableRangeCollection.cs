﻿namespace System.Windows.Data;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

public class WpfObservableRangeCollection<T> : RangeObservableCollection<T>
{
    DeferredEventsCollection? _deferredEvents;

    public WpfObservableRangeCollection()
    {
    }

    public WpfObservableRangeCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    public WpfObservableRangeCollection(List<T> list) : base(list)
    {
    }


    /// <summary>
    /// Raise CollectionChanged event to any listeners.
    /// Properties/methods modifying this ObservableCollection will raise
    /// a collection changed event through this virtual method.
    /// </summary>
    /// <remarks>
    /// When overriding this method, either call its base implementation
    /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
    /// </remarks>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        var deferredEvents = (ICollection<NotifyCollectionChangedEventArgs>?)typeof(RangeObservableCollection<T>)?.GetField("_deferredEvents", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(this);
        if (deferredEvents is not null)
        {
            deferredEvents.Add(e);
            return;
        }

        foreach (var handler in GetHandlers())
        {
            if (WpfObservableRangeCollection<T>.IsRange(e) && handler.Target is CollectionView cv)
            {
                cv.Dispatcher.Invoke(() => cv.Refresh());
            }
            else
            {
                handler(this, e);
            }
        }
    }

    protected override IDisposable DeferEvents()
    {
        return new DeferredEventsCollection(this);
    }

    static bool IsRange(NotifyCollectionChangedEventArgs e)
    {
        return e.NewItems?.Count > 1 || e.OldItems?.Count > 1;
    }

    IEnumerable<NotifyCollectionChangedEventHandler> GetHandlers()
    {
        var info = typeof(ObservableCollection<T>).GetField(nameof(CollectionChanged), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException();
        var @event = (MulticastDelegate?)info.GetValue(this);
        return @event?.GetInvocationList()
          .Cast<NotifyCollectionChangedEventHandler>()
          .Distinct()
          ?? Enumerable.Empty<NotifyCollectionChangedEventHandler>();
    }

    class DeferredEventsCollection : List<NotifyCollectionChangedEventArgs>, IDisposable
    {
        private readonly WpfObservableRangeCollection<T> _collection;
        public DeferredEventsCollection(WpfObservableRangeCollection<T> collection)
        {
            Debug.Assert(collection is not null);
            Debug.Assert(collection._deferredEvents is null);
            _collection = collection;
            _collection._deferredEvents = this;
        }

        public void Dispose()
        {
            _collection._deferredEvents = null;

            var handlers = _collection
              .GetHandlers()
              .ToLookup(h => h.Target is CollectionView);

            foreach (var handler in handlers[false])
                foreach (var e in this)
                    handler(_collection, e);

            foreach (var cv in handlers[true]
              .Select(h => h.Target)
              .Cast<CollectionView>()
              .Distinct())
                cv.Refresh();
        }
    }
}