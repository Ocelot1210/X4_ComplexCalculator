using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic;
using System.Windows.Data;
using System.Windows.Threading;
using System.Collections.Specialized;
using X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support;


namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Querying
{
    public class QueryController
    {
        private readonly object _LockObject = new();


        private readonly Dictionary<string, FilterData> _FiltersForColumns = new();


        private Query _Query = new();


        public FilterData ColumnFilterData { get; set; }

        public IEnumerable ItemsSource { get; set; }


        public Dispatcher CallingThreadDispatcher { get; set; }


        public bool UseBackgroundWorker { get; set; }


        public QueryController(FilterData columnFilterData, IEnumerable itemsSource, Dispatcher callingThreadDispatcher, bool useBackGroundWorker)
        {
            ColumnFilterData = columnFilterData;
            ItemsSource = itemsSource;
            CallingThreadDispatcher = callingThreadDispatcher;
            UseBackgroundWorker = useBackGroundWorker;
        }


        public void DoQuery(bool force = false)
        {
            ColumnFilterData.IsSearchPerformed = false;

            if (!_FiltersForColumns.ContainsKey(ColumnFilterData.ValuePropertyBindingPath))
            {
                _FiltersForColumns.Add(ColumnFilterData.ValuePropertyBindingPath, ColumnFilterData);
            }
            else
            {
                _FiltersForColumns[ColumnFilterData.ValuePropertyBindingPath] = ColumnFilterData;
            }

            if (IsRefresh)
            {
                if (_FiltersForColumns.ElementAt(_FiltersForColumns.Count - 1).Value.ValuePropertyBindingPath
                    == ColumnFilterData.ValuePropertyBindingPath)
                {
                    RunFiltering(force);
                }
            }
            else if (FilteringNeeded)
            {
                RunFiltering(force);
            }

            ColumnFilterData.IsSearchPerformed = true;
            ColumnFilterData.IsRefresh = false;
        }

        public bool IsCurentControlFirstControl
        {
            get => _FiltersForColumns.Count > 0 && 
                        _FiltersForColumns.ElementAt(0).Value.ValuePropertyBindingPath == ColumnFilterData.ValuePropertyBindingPath;
        }

        public void ClearFilter()
        {
            int count = _FiltersForColumns.Count;

            for(int i = 0; i < count; i++)
            {
                _FiltersForColumns.ElementAt(i).Value.ClearData();
            }

            DoQuery();
        }

        #region Internal

        private bool IsRefresh
        {
            get => _FiltersForColumns.Any(x => x.Value.IsRefresh);
        }

        private bool FilteringNeeded
        {
            get => _FiltersForColumns.Count(x => !x.Value.IsSearchPerformed) == 1;
        }

        private void RunFiltering(bool force)
        {
            bool filterChanged;

            CreateFilterExpressionsAndFilteredCollection(out filterChanged, force);

            if (filterChanged || force)
            {
                OnFilteringStarted(this, EventArgs.Empty);

                ApplayFilter();
            }
        }

        private void CreateFilterExpressionsAndFilteredCollection(out bool filterChanged, bool force)
        {
            var queryCreator = new QueryCreator(_FiltersForColumns);

            queryCreator.CreateFilter(ref _Query);

            filterChanged = (_Query.IsQueryChanged || (_Query.FilterString != "" && IsRefresh));

            if ((force && _Query.FilterString != "") || 
                (_Query.FilterString != "" && filterChanged))
            {
                var collection = ItemsSource;

                if (ItemsSource is ListCollectionView col)
                {
                    collection = col.SourceCollection;
                }


                if (ItemsSource is INotifyCollectionChanged observable)
                {
                    observable.CollectionChanged -= new NotifyCollectionChangedEventHandler(Observable_CollectionChanged);
                    observable.CollectionChanged += new NotifyCollectionChangedEventHandler(Observable_CollectionChanged);
                }

                #region Debug
#if DEBUG
                System.Diagnostics.Debug.WriteLine("QUERY STATEMENT: " + _Query.FilterString);

                string debugParameters = String.Empty;
                _Query.QueryParameters.ForEach(p =>
                {
                    if (debugParameters.Length > 0) debugParameters += ",";
                    debugParameters += p?.ToString();
                });

                System.Diagnostics.Debug.WriteLine("QUERY PARAMETRS: " + debugParameters);
                #endif
                #endregion


                if (_Query.FilterString != "")
                {
                    var result = collection.AsQueryable().Where(_Query.FilterString, _Query.QueryParameters.ToArray<object?>());

                    filteredCollection = result.OfType<object?>().ToList();
                }
            }
            else
            {
                filteredCollection = null;
            }

            _Query.StoreLastUsedValues();
        }

        private void Observable_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            => DoQuery(true);


        #region Internal Filtering

        private IList? filteredCollection;
        HashSet<object> filteredCollectionHashSet = new();

        void ApplayFilter()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);

            if (filteredCollection != null)
            {
                ExecuteFilterAction(() =>
                {
                    filteredCollectionHashSet = InitLookupDictionary(filteredCollection);
 
                    view.Filter = new Predicate<object>(itemPassesFilter);

                    OnFilteringFinished(this, EventArgs.Empty);
                });
            }
            else
            {
                ExecuteFilterAction(() =>
                {
                    if (view.Filter != null)
                    {
                        view.Filter = null;
                    }

                    OnFilteringFinished(this, EventArgs.Empty);
                });
            }
        }

        private void ExecuteFilterAction(Action action)
        {
            if (UseBackgroundWorker)
            {
                var worker = new BackgroundWorker();

                worker.DoWork += delegate(object sender, DoWorkEventArgs e)
                {
                    lock (_LockObject)
                    {
                        ExecuteActionUsingDispatcher(action);
                    }
                };

                worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
                {
                    if (e.Error != null)
                    {
                        OnFilteringError(this, new FilteringEventArgs(e.Error));
                    }
                };

                worker.RunWorkerAsync();
            }
            else
            {
                try
                {
                    ExecuteActionUsingDispatcher(action);
                }
                catch (Exception e)
                {
                    OnFilteringError(this, new FilteringEventArgs(e));
                }
            }
        }

        private void ExecuteActionUsingDispatcher(Action action)
        {
            if (CallingThreadDispatcher != null && !CallingThreadDispatcher.CheckAccess())
            {
                CallingThreadDispatcher.Invoke(() => Invoke(action));
            }
            else
            {
                Invoke(action);
            }
        }

        private static void Invoke(Action action)
        {
            Trace.WriteLine("------------------ START APPLAY FILTER ------------------------------");
            Stopwatch sw = Stopwatch.StartNew();

            action.Invoke();

            sw.Stop();
            Trace.WriteLine("TIME: " + sw.ElapsedMilliseconds);
            Trace.WriteLine("------------------ STOP APPLAY FILTER ------------------------------");
        }

        private bool itemPassesFilter(object item)
        {
            return filteredCollectionHashSet.Contains(item);
        }

        #region Helpers
        private HashSet<object> InitLookupDictionary(IList collection)
        {
            HashSet<object> dictionary;

            if (collection != null)
            {
                dictionary = new HashSet<object>(collection.Cast<object>());
            }
            else
            {
                dictionary = new HashSet<object>();
            }

            return dictionary;
        }
        #endregion

        #endregion
        #endregion

        #region Progress Notification
        public event EventHandler<EventArgs>? FilteringStarted;
        public event EventHandler<EventArgs>? FilteringFinished;
        public event EventHandler<FilteringEventArgs>? FilteringError;

        private void OnFilteringStarted(object sender, EventArgs e)
        {
            FilteringStarted?.Invoke(sender, e);
        }

        private void OnFilteringFinished(object sender, EventArgs e)
        {
            FilteringFinished?.Invoke(sender, e);
        }

        private void OnFilteringError(object sender, FilteringEventArgs e)
        {
            FilteringError?.Invoke(sender, e);
        }
        #endregion
    }
}
