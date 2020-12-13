using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support;
using System.Collections;
using System.Windows.Data;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Querying
{
    public class QueryController
    {
        public FilterData ColumnFilterData { get; set; }
        public IEnumerable ItemsSource { get; set; }

        private readonly Dictionary<string, FilterData> filtersForColumns;
        private Query query;

        public Dispatcher CallingThreadDispatcher { get; set; }
        public bool UseBackgroundWorker { get; set; }
        private readonly object lockObject;

        public QueryController()
        {
            lockObject = new object();

            filtersForColumns = new Dictionary<string, FilterData>();
            query = new Query();
        }

        public void DoQuery()
        {
            DoQuery(false);
        }

        public void DoQuery(bool force)
        {
            ColumnFilterData.IsSearchPerformed = false;

            if (!filtersForColumns.ContainsKey(ColumnFilterData.ValuePropertyBindingPath))
            {
                filtersForColumns.Add(ColumnFilterData.ValuePropertyBindingPath, ColumnFilterData);
            }
            else
            {
                filtersForColumns[ColumnFilterData.ValuePropertyBindingPath] = ColumnFilterData;
            }

            if (isRefresh)
            {
                if (filtersForColumns.ElementAt(filtersForColumns.Count - 1).Value.ValuePropertyBindingPath
                    == ColumnFilterData.ValuePropertyBindingPath)
                {
                    runFiltering(force);
                }
            }
            else if (filteringNeeded)
            {
                runFiltering(force);
            }

            ColumnFilterData.IsSearchPerformed = true;
            ColumnFilterData.IsRefresh = false;
        }

        public bool IsCurentControlFirstControl => filtersForColumns.Count > 0
    ? filtersForColumns.ElementAt(0).Value.ValuePropertyBindingPath == ColumnFilterData.ValuePropertyBindingPath : false;

        public void ClearFilter()
        {
            int count = filtersForColumns.Count;
            for (int i = 0; i < count; i++)
            {
                FilterData data = filtersForColumns.ElementAt(i).Value;

                data.ClearData();
            }

            DoQuery();
        }

        #region Internal

        private bool isRefresh => (from f in filtersForColumns where f.Value.IsRefresh select f).Any();

        private bool filteringNeeded => (from f in filtersForColumns where !f.Value.IsSearchPerformed select f).Count() == 1;

        private void runFiltering(bool force)
        {
            bool filterChanged;

            createFilterExpressionsAndFilteredCollection(out filterChanged, force);

            if (filterChanged || force)
            {
                OnFilteringStarted(this, EventArgs.Empty);

                applayFilter();
            }
        }

        private void createFilterExpressionsAndFilteredCollection(out bool filterChanged, bool force)
        {
            QueryCreator queryCreator = new QueryCreator(filtersForColumns);

            queryCreator.CreateFilter(ref query);

            filterChanged = query.IsQueryChanged || (query.FilterString != String.Empty && isRefresh);

            if ((force && query.FilterString != String.Empty) || (query.FilterString != String.Empty && filterChanged))
            {
                IEnumerable collection = ItemsSource as IEnumerable;

                if (ItemsSource is ListCollectionView)
                {
                    collection = (ItemsSource as ListCollectionView).SourceCollection as IEnumerable;
                }

                var observable = ItemsSource as System.Collections.Specialized.INotifyCollectionChanged;
                if (observable != null)
                {
                    observable.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(observable_CollectionChanged);
                    observable.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(observable_CollectionChanged);
                }

//                #region Debug
//#if DEBUG
//                System.Diagnostics.Debug.WriteLine("QUERY STATEMENT: " + query.FilterString);

//                string debugParameters = String.Empty;
//                query.QueryParameters.ForEach(p =>
//                {
//                    if (debugParameters.Length > 0) debugParameters += ",";
//                    debugParameters += p.ToString();
//                });

//                System.Diagnostics.Debug.WriteLine("QUERY PARAMETRS: " + debugParameters);
//#endif
//                #endregion

                if (query.FilterString != String.Empty)
                {
                    var result = collection.AsQueryable().Where(query.FilterString, query.QueryParameters.ToArray<object>());

                    filteredCollection = result.Cast<object>().ToList();
                }
            }
            else
            {
                filteredCollection = null;
            }

            query.StoreLastUsedValues();
        }

        private void observable_CollectionChanged(
            object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DoQuery(true);
        }

        #region Internal Filtering

        private IList filteredCollection;
        private HashSet<object> filteredCollectionHashSet;

        private void CommitEdit(ICollectionView view)
        {
            if (view is IEditableCollectionView)
            {
                IEditableCollectionView editableView = view as IEditableCollectionView;

                if (editableView.IsAddingNew || editableView.IsEditingItem)
                {
                    editableView.CommitEdit();
                    editableView.CommitEdit();
                }
            }
        }

        private void applayFilter()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);

            CommitEdit(view);
            if (filteredCollection != null)
            {
                executeFilterAction(
                    new Action(() =>
                    {
                        filteredCollectionHashSet = initLookupDictionary(filteredCollection);

                        view.Filter = new Predicate<object>(itemPassesFilter);

                        OnFilteringFinished(this, EventArgs.Empty);
                    })
                );
            }
            else
            {
                executeFilterAction(
                    new Action(() =>
                    {
                        if (view.Filter != null)
                        {
                            view.Filter = null;
                        }

                        OnFilteringFinished(this, EventArgs.Empty);
                    })
                );
            }
        }

        private void executeFilterAction(Action action)
        {
            if (UseBackgroundWorker)
            {
                BackgroundWorker worker = new BackgroundWorker();

                worker.DoWork += (object sender, DoWorkEventArgs e) =>
                {
                    lock (lockObject)
                    {
                        executeActionUsingDispatcher(action);
                    }
                };

                worker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) =>
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
                    executeActionUsingDispatcher(action);
                }
                catch (Exception e)
                {
                    OnFilteringError(this, new FilteringEventArgs(e));
                }
            }
        }

        private void executeActionUsingDispatcher(Action action)
        {
            if (this.CallingThreadDispatcher?.CheckAccess() == false)
            {
                this.CallingThreadDispatcher.Invoke
                    (
                        new Action(() => invoke(action))
                    );
            }
            else
            {
                invoke(action);
            }
        }

        private static void invoke(Action action)
        {
            action.Invoke();
        }

        private bool itemPassesFilter(object item) => filteredCollectionHashSet.Contains(item);

        #region Helpers
        private HashSet<object> initLookupDictionary(IList collection)
        {
            HashSet<object> dictionary;

            if (collection != null)
            {
                dictionary = new HashSet<object>(collection.Cast<object>()/*.ToList()*/);
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
        public event EventHandler<EventArgs> FilteringStarted;
        public event EventHandler<EventArgs> FilteringFinished;
        public event EventHandler<FilteringEventArgs> FilteringError;

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
