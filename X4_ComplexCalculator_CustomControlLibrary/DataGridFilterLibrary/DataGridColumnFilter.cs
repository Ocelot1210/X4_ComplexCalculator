using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Querying;
using X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary
{
    public class DataGridColumnFilter : Control
    {
        static DataGridColumnFilter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridColumnFilter), new FrameworkPropertyMetadata(typeof(DataGridColumnFilter)));
        }

        #region Overrides
        protected override void OnPropertyChanged(
            DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == DataGridItemsSourceProperty
                && e.OldValue != e.NewValue
                && AssignedDataGridColumn is not null && DataGrid is not null && AssignedDataGridColumn is DataGridColumn)
            {
                Initialize();

                //query optimization filed
                FilterCurrentData.IsRefresh = true;

                //init query
                FilterCurrentData_FilterChangedEvent(this, EventArgs.Empty);

                FilterCurrentData.FilterChangedEvent -= new EventHandler<EventArgs>(FilterCurrentData_FilterChangedEvent);
                FilterCurrentData.FilterChangedEvent += new EventHandler<EventArgs>(FilterCurrentData_FilterChangedEvent);
            }

            base.OnPropertyChanged(e);
        }
        #endregion

        #region Properties

        public static readonly DependencyProperty FilterCurrentDataProperty =
            DependencyProperty.Register(nameof(FilterCurrentData), typeof(FilterData), typeof(DataGridColumnFilter));

        public FilterData FilterCurrentData
        {
            get => (FilterData)GetValue(FilterCurrentDataProperty);
            set => SetValue(FilterCurrentDataProperty, value);
        }




        public static readonly DependencyProperty AssignedDataGridColumnHeaderProperty =
            DependencyProperty.Register(nameof(AssignedDataGridColumnHeader), typeof(DataGridColumnHeader), typeof(DataGridColumnFilter));

        public DataGridColumnHeader AssignedDataGridColumnHeader
        {
            get => (DataGridColumnHeader)GetValue(AssignedDataGridColumnHeaderProperty);
            set => SetValue(AssignedDataGridColumnHeaderProperty, value);
        }




        public static readonly DependencyProperty AssignedDataGridColumnProperty =
            DependencyProperty.Register(nameof(AssignedDataGridColumn), typeof(DataGridColumn), typeof(DataGridColumnFilter));

        public DataGridColumn AssignedDataGridColumn
        {
            get => (DataGridColumn)GetValue(AssignedDataGridColumnProperty);
            set => SetValue(AssignedDataGridColumnProperty, value);
        }




        public static readonly DependencyProperty DataGridProperty =
            DependencyProperty.Register(nameof(DataGrid), typeof(DataGrid), typeof(DataGridColumnFilter));

        public DataGrid DataGrid
        {
            get => (DataGrid)GetValue(DataGridProperty);
            set => SetValue(DataGridProperty, value);
        }




        public static readonly DependencyProperty DataGridItemsSourceProperty =
            DependencyProperty.Register(nameof(DataGridItemsSource), typeof(IEnumerable), typeof(DataGridColumnFilter));

        public IEnumerable DataGridItemsSource
        {
            get => (IEnumerable)GetValue(DataGridItemsSourceProperty);
            set => SetValue(DataGridItemsSourceProperty, value);
        }




        public static readonly DependencyProperty IsFilteringInProgressProperty =
            DependencyProperty.Register(nameof(IsFilteringInProgress), typeof(bool), typeof(DataGridColumnFilter));
        public bool IsFilteringInProgress
        {
            get => (bool)GetValue(IsFilteringInProgressProperty);
            set => SetValue(IsFilteringInProgressProperty, value);
        }




        public FilterType FilterType => FilterCurrentData != null ? FilterCurrentData.Type : FilterType.Text;



        public static readonly DependencyProperty IsTextFilterControlProperty =
            DependencyProperty.Register("IsTextFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsTextFilterControl
        {
            get => (bool)GetValue(IsTextFilterControlProperty);
            set => SetValue(IsTextFilterControlProperty, value);
        }




        public static readonly DependencyProperty IsNumericFilterControlProperty =
            DependencyProperty.Register("IsNumericFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsNumericFilterControl
        {
            get => (bool)GetValue(IsNumericFilterControlProperty);
            set => SetValue(IsNumericFilterControlProperty, value);
        }




        public static readonly DependencyProperty IsNumericBetweenFilterControlProperty =
            DependencyProperty.Register("IsNumericBetweenFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsNumericBetweenFilterControl
        {
            get => (bool)GetValue(IsNumericBetweenFilterControlProperty);
            set => SetValue(IsNumericBetweenFilterControlProperty, value);
        }




        public static readonly DependencyProperty IsBooleanFilterControlProperty =
            DependencyProperty.Register("IsBooleanFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsBooleanFilterControl
        {
            get => (bool)GetValue(IsBooleanFilterControlProperty);
            set => SetValue(IsBooleanFilterControlProperty, value);
        }




        public static readonly DependencyProperty IsListFilterControlProperty =
            DependencyProperty.Register("IsListFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsListFilterControl
        {
            get => (bool)GetValue(IsListFilterControlProperty);
            set => SetValue(IsListFilterControlProperty, value);
        }




        public static readonly DependencyProperty IsDateTimeFilterControlProperty =
            DependencyProperty.Register("IsDateTimeFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsDateTimeFilterControl
        {
            get => (bool)GetValue(IsDateTimeFilterControlProperty);
            set => SetValue(IsDateTimeFilterControlProperty, value);
        }




        public static readonly DependencyProperty IsDateTimeBetweenFilterControlProperty =
            DependencyProperty.Register("IsDateTimeBetweenFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsDateTimeBetweenFilterControl
        {
            get => (bool)GetValue(IsDateTimeBetweenFilterControlProperty);
            set => SetValue(IsDateTimeBetweenFilterControlProperty, value);
        }




        public static readonly DependencyProperty IsFirstFilterControlProperty =
            DependencyProperty.Register("IsFirstFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsFirstFilterControl
        {
            get => (bool)GetValue(IsFirstFilterControlProperty);
            set => SetValue(IsFirstFilterControlProperty, value);
        }




        public static readonly DependencyProperty IsControlInitializedProperty =
            DependencyProperty.Register("IsControlInitialized", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsControlInitialized
        {
            get => (bool)GetValue(IsControlInitializedProperty);
            set => SetValue(IsControlInitializedProperty, value);
        }
        #endregion



        #region Initialization
        private void Initialize()
        {
            if (DataGridItemsSource is not null && AssignedDataGridColumn is not null && DataGrid is not null)
            {
                InitFilterData();

                InitControlType();

                HandleListFilterType();

                HookUpCommands();

                IsControlInitialized = true;
            }
        }


        private void InitFilterData()
        {
            if (FilterCurrentData is null || !FilterCurrentData.IsTypeInitialized)
            {
                string valuePropertyBindingPath = GetValuePropertyBindingPath(AssignedDataGridColumn);


                Type valuePropertyType = GetValuePropertyType(
                    valuePropertyBindingPath, GetItemSourceElementType(out bool typeInitialized));

                FilterType filterType = GetFilterType(
                    valuePropertyType, 
                    IsComboDataGridColumn(),
                    IsBetweenType());

                var filterOperator = FilterOperator.Undefined;

                string queryString = "";
                string queryStringTo = "";


                FilterCurrentData = new FilterData(
                    filterOperator, 
                    filterType, 
                    valuePropertyBindingPath, 
                    valuePropertyType, 
                    queryString, 
                    queryStringTo,
                    typeInitialized,
                    DataGridColumnExtensions.GetIsCaseSensitiveSearch(AssignedDataGridColumn));
            }
        }

        private void InitControlType()
        {
            IsFirstFilterControl    = false;

            IsTextFilterControl     = false;
            IsNumericFilterControl  = false;
            IsBooleanFilterControl  = false;
            IsListFilterControl     = false;
            IsDateTimeFilterControl = false;

            IsNumericBetweenFilterControl = false;
            IsDateTimeBetweenFilterControl = false;

            switch (FilterType)
            {
                case FilterType.Text:
                    IsTextFilterControl = true;
                    break;

                case FilterType.Numeric:
                    IsNumericFilterControl = true;
                    break;

                case FilterType.List:
                    IsListFilterControl = true;
                    break;

                case FilterType.NumericBetween:
                    IsNumericBetweenFilterControl = true;
                    break;

                case FilterType.DateTimeBetween:
                    IsDateTimeBetweenFilterControl = true;
                    break;

                default:
                    break;
            }
        }

        private void HandleListFilterType()
        {
            if (FilterCurrentData.Type == FilterType.List)
            {
                if (Template.FindName("PART_ComboBoxFilter", this) is ComboBox comboBox &&
                    AssignedDataGridColumn is DataGridComboBoxColumn column)
                {

                    if (DataGridComboBoxExtensions.GetIsTextFilter(column))
                    {
                        FilterCurrentData.Type = FilterType.Text;
                        InitControlType();
                        return;
                    }


                    //list filter type

                    var columnItemsSourceBinding = BindingOperations.GetBinding(column, DataGridComboBoxColumn.ItemsSourceProperty);

                    if (columnItemsSourceBinding == null)
                    {
                        if (column.EditingElementStyle.Setters.First(s => ((Setter)s).Property == DataGridComboBoxColumn.ItemsSourceProperty) is Setter styleSetter)
                        {
                            columnItemsSourceBinding = styleSetter.Value as Binding;
                        }
                    }

                    comboBox.DisplayMemberPath = column.DisplayMemberPath;
                    comboBox.SelectedValuePath = column.SelectedValuePath;

                    if (columnItemsSourceBinding != null)
                    {
                        BindingOperations.SetBinding(comboBox, ComboBox.ItemsSourceProperty, columnItemsSourceBinding);
                    }

                    comboBox.RequestBringIntoView
                        += new RequestBringIntoViewEventHandler(setComboBindingAndHanldeUnsetValue);
                }
            }
        }

        private void setComboBindingAndHanldeUnsetValue(object sender, RequestBringIntoViewEventArgs e)
        {
            if (sender is not ComboBox combo ||
                AssignedDataGridColumn is not DataGridComboBoxColumn column)
            {
                return;
            }

            if (column.ItemsSource == null)
            {
                if (combo?.ItemsSource != null)
                {
                    var list = combo.ItemsSource.Cast<object>().ToList();

                    if (list.Any() && list[0] != DependencyProperty.UnsetValue)
                    {
                        combo.RequestBringIntoView -=
                            new RequestBringIntoViewEventHandler(setComboBindingAndHanldeUnsetValue);

                        list.Insert(0, DependencyProperty.UnsetValue);

                        combo.DisplayMemberPath = column?.DisplayMemberPath;
                        combo.SelectedValuePath = column?.SelectedValuePath;

                        combo.ItemsSource = list;
                    }
                }
            }
            else
            {
                combo.RequestBringIntoView -=
                    new RequestBringIntoViewEventHandler(setComboBindingAndHanldeUnsetValue);

                IList? comboList = null;
                IList? columnList = null;

                if (combo.ItemsSource != null)
                {
                    comboList = combo.ItemsSource.Cast<object>().ToList();
                }

                columnList = column.ItemsSource.Cast<object>().ToList();

                if (comboList == null ||
                    (columnList.Count > 0 && columnList.Count + 1 != comboList.Count))
                {
                    columnList = column.ItemsSource.Cast<object>().ToList();
                    columnList.Insert(0, DependencyProperty.UnsetValue);

                    combo.ItemsSource = columnList;
                }

                combo.RequestBringIntoView +=
                    new RequestBringIntoViewEventHandler(setComboBindingAndHanldeUnsetValue);
            }
        }

        private string GetValuePropertyBindingPath(DataGridColumn column)
        {
            var path = "";

            if (column is DataGridBoundColumn bc)
            {
                return (bc.Binding as Binding)?.Path.Path ?? "";
            }

            if (column is DataGridTemplateColumn tc)
            {
                var templateContent = tc.CellTemplate.LoadContent();

                if (templateContent is not null && templateContent is TextBlock textBlock)
                {
                    BindingExpression binding = textBlock.GetBindingExpression(TextBlock.TextProperty);

                    return binding.ParentBinding.Path.Path;
                }
                return "";
            }

            if (column is DataGridComboBoxColumn comboColumn)
            {
                path = ((comboColumn.SelectedValueBinding as Binding) ??
                        (comboColumn.SelectedItemBinding as Binding) ??
                        (comboColumn.SelectedValueBinding as Binding))?.Path.Path;

                if (comboColumn.SelectedItemBinding is not null && comboColumn.SelectedValueBinding is null)
                {
                    if (path is not null && path.Trim().Length > 0)
                    {
                        if (DataGridComboBoxExtensions.GetIsTextFilter(comboColumn))
                        {
                            path += "." + comboColumn.DisplayMemberPath; 
                        }
                        else
                        {
                            path += "." + comboColumn.SelectedValuePath;
                        }
                    }
                }

                return path ?? "";
            }


            return path;
        }

        private Type GetValuePropertyType(string path, Type? elementType)
        {
            var type = typeof(object);

            if (elementType is not null)
            {
                var properties = path.Split(".".ToCharArray()[0]);

                PropertyInfo? pi = null;

                if (properties.Length == 1)
                {
                    pi = elementType.GetProperty(path);
                }
                else
                {
                    pi = elementType.GetProperty(properties[0]);

                    for (int i = 1; i < properties.Length; i++)
                    {
                        if (pi != null)
                        {
                            pi = pi.PropertyType.GetProperty(properties[i]);
                        }
                    }
                }


                if (pi != null)
                {
                    type = pi.PropertyType;
                }
            }

            return type;
        }

        private Type? GetItemSourceElementType(out bool typeInitialized)
        {
            typeInitialized = false;

            Type? elementType = null;

            var l = DataGridItemsSource as IList;

            if (l is not null && l.Count > 0)
            {
                var obj = l[0];

                if (obj is not null)
                {
                    elementType = obj.GetType();
                    typeInitialized = true;
                }
                else
                {
                    elementType = typeof(object);
                }
            }

            if (l is null)
            {
                if (DataGridItemsSource is ListCollectionView lw && lw.Count > 0)
                {
                    var obj = lw.CurrentItem;

                    if (obj is not null)
                    {
                        elementType = lw.CurrentItem.GetType();
                        typeInitialized = true;
                    }
                    else
                    {
                        elementType = typeof(object);
                    }
                }
            }

            return elementType;
        }


        private FilterType GetFilterType(
            Type valuePropertyType, 
            bool isAssignedDataGridColumnComboDataGridColumn,
            bool isBetweenType)
        {
            Type type = Nullable.GetUnderlyingType(valuePropertyType) ??
                        valuePropertyType;

            FilterType filterType;

            if (isAssignedDataGridColumnComboDataGridColumn)
            {
                filterType = FilterType.List;
            }
            else
            {
                filterType = Type.GetTypeCode(type) switch
                {
                    TypeCode.Boolean => FilterType.Boolean,
                    TypeCode.SByte => FilterType.Numeric,
                    TypeCode.Byte => FilterType.Numeric,
                    TypeCode.Int16 => FilterType.Numeric,
                    TypeCode.UInt16 => FilterType.Numeric,
                    TypeCode.Int32 => FilterType.Numeric,
                    TypeCode.UInt32 => FilterType.Numeric,
                    TypeCode.Int64 => FilterType.Numeric,
                    TypeCode.UInt64 => FilterType.Numeric,
                    TypeCode.Single => FilterType.Numeric,
                    TypeCode.Decimal => FilterType.Numeric,
                    TypeCode.Double => FilterType.Numeric,
                    TypeCode.DateTime => FilterType.DateTime,
                    _ => FilterType.Text
                };
            }

            if (filterType == FilterType.Numeric && isBetweenType)
            {
                filterType = FilterType.NumericBetween;
            }
            else if (filterType == FilterType.DateTime && isBetweenType)
            {
                filterType = FilterType.DateTimeBetween;
            }

            return filterType;
        }

        private bool IsComboDataGridColumn()
        {
            return AssignedDataGridColumn is DataGridComboBoxColumn;
        }

        private bool IsBetweenType()
        {
            return DataGridColumnExtensions.GetIsBetweenFilterControl(AssignedDataGridColumn);
        }

        private void HookUpCommands()
        {
            if (DataGridExtensions.GetClearFilterCommand(DataGrid) == null)
            {
                DataGridExtensions.SetClearFilterCommand(DataGrid, new DataGridFilterCommand(ClearQuery));
            }
        }
        #endregion


        #region Querying
        void FilterCurrentData_FilterChangedEvent(object? sender, EventArgs e)
        {
            if (DataGrid != null)
            {
                QueryController query = QueryControllerFactory.GetQueryController(
                    DataGrid, FilterCurrentData, DataGridItemsSource);

                AddFilterStateHandlers(query);

                query.DoQuery();

                IsFirstFilterControl = query.IsCurentControlFirstControl;
            }
        }

        private void ClearQuery(object parameter)
        {
            if (DataGrid != null)
            {
                var query = QueryControllerFactory.GetQueryController(DataGrid, FilterCurrentData, DataGridItemsSource);

                query.ClearFilter();
            }
        }

        private void AddFilterStateHandlers(QueryController query)
        {
            query.FilteringStarted -= new EventHandler<EventArgs>(Query_FilteringStarted);
            query.FilteringFinished -= new EventHandler<EventArgs>(Query_FilteringFinished);

            query.FilteringStarted += new EventHandler<EventArgs>(Query_FilteringStarted);
            query.FilteringFinished += new EventHandler<EventArgs>(Query_FilteringFinished);
        }

        void Query_FilteringFinished(object? sender, EventArgs e)
        {
            if (FilterCurrentData.Equals((sender as QueryController)?.ColumnFilterData))
            {
                IsFilteringInProgress = false;
            }
        }

        void Query_FilteringStarted(object? sender, EventArgs e)
        {
            if (FilterCurrentData.Equals((sender as QueryController)?.ColumnFilterData))
            {
                IsFilteringInProgress = true;
            }
        }
        #endregion
    }
}