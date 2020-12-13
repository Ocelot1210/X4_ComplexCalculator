﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support;
using System.Reflection;
using X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Querying;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary;

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
                && AssignedDataGridColumn != null && DataGrid != null && AssignedDataGridColumn is DataGridColumn)
            {
                initialize();

                FilterCurrentData.IsRefresh = true;//query optimization filed

                filterCurrentData_FilterChangedEvent(this, EventArgs.Empty);//init query

                FilterCurrentData.FilterChangedEvent -= filterCurrentData_FilterChangedEvent;
                FilterCurrentData.FilterChangedEvent += filterCurrentData_FilterChangedEvent;
            }

            base.OnPropertyChanged(e);
        }
        #endregion

        #region Properties
        public FilterData FilterCurrentData
        {
            get { return (FilterData)GetValue(FilterCurrentDataProperty); }
            set { SetValue(FilterCurrentDataProperty, value); }
        }

        public static readonly DependencyProperty FilterCurrentDataProperty =
            DependencyProperty.Register("FilterCurrentData", typeof(FilterData), typeof(DataGridColumnFilter));

        public DataGridColumnHeader AssignedDataGridColumnHeader
        {
            get { return (DataGridColumnHeader)GetValue(AssignedDataGridColumnHeaderProperty); }
            set { SetValue(AssignedDataGridColumnHeaderProperty, value); }
        }

        public static readonly DependencyProperty AssignedDataGridColumnHeaderProperty =
            DependencyProperty.Register("AssignedDataGridColumnHeader", typeof(DataGridColumnHeader), typeof(DataGridColumnFilter));

        public DataGridColumn AssignedDataGridColumn
        {
            get { return (DataGridColumn)GetValue(AssignedDataGridColumnProperty); }
            set { SetValue(AssignedDataGridColumnProperty, value); }
        }

        public static readonly DependencyProperty AssignedDataGridColumnProperty =
            DependencyProperty.Register("AssignedDataGridColumn", typeof(DataGridColumn), typeof(DataGridColumnFilter));

        public DataGrid DataGrid
        {
            get { return (DataGrid)GetValue(DataGridProperty); }
            set { SetValue(DataGridProperty, value); }
        }

        public static readonly DependencyProperty DataGridProperty =
            DependencyProperty.Register("DataGrid", typeof(DataGrid), typeof(DataGridColumnFilter));

        public IEnumerable DataGridItemsSource
        {
            get { return (IEnumerable)GetValue(DataGridItemsSourceProperty); }
            set { SetValue(DataGridItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty DataGridItemsSourceProperty =
            DependencyProperty.Register("DataGridItemsSource", typeof(IEnumerable), typeof(DataGridColumnFilter));

        public bool IsFilteringInProgress
        {
            get { return (bool)GetValue(IsFilteringInProgressProperty); }
            set { SetValue(IsFilteringInProgressProperty, value); }
        }

        public static readonly DependencyProperty IsFilteringInProgressProperty =
            DependencyProperty.Register("IsFilteringInProgress", typeof(bool), typeof(DataGridColumnFilter));

        public FilterType FilterType => FilterCurrentData != null ? FilterCurrentData.Type : FilterType.Text;

        public bool IsTextFilterControl
        {
            get { return (bool)GetValue(IsTextFilterControlProperty); }
            set { SetValue(IsTextFilterControlProperty, value); }
        }

        public static readonly DependencyProperty IsTextFilterControlProperty =
            DependencyProperty.Register("IsTextFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsNumericFilterControl
        {
            get { return (bool)GetValue(IsNumericFilterControlProperty); }
            set { SetValue(IsNumericFilterControlProperty, value); }
        }

        public static readonly DependencyProperty IsNumericFilterControlProperty =
            DependencyProperty.Register("IsNumericFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsNumericBetweenFilterControl
        {
            get { return (bool)GetValue(IsNumericBetweenFilterControlProperty); }
            set { SetValue(IsNumericBetweenFilterControlProperty, value); }
        }

        public static readonly DependencyProperty IsNumericBetweenFilterControlProperty =
            DependencyProperty.Register("IsNumericBetweenFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsBooleanFilterControl
        {
            get { return (bool)GetValue(IsBooleanFilterControlProperty); }
            set { SetValue(IsBooleanFilterControlProperty, value); }
        }

        public static readonly DependencyProperty IsBooleanFilterControlProperty =
            DependencyProperty.Register("IsBooleanFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsListFilterControl
        {
            get { return (bool)GetValue(IsListFilterControlProperty); }
            set { SetValue(IsListFilterControlProperty, value); }
        }

        public static readonly DependencyProperty IsListFilterControlProperty =
            DependencyProperty.Register("IsListFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsDateTimeFilterControl
        {
            get { return (bool)GetValue(IsDateTimeFilterControlProperty); }
            set { SetValue(IsDateTimeFilterControlProperty, value); }
        }

        public static readonly DependencyProperty IsDateTimeFilterControlProperty =
            DependencyProperty.Register("IsDateTimeFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsDateTimeBetweenFilterControl
        {
            get { return (bool)GetValue(IsDateTimeBetweenFilterControlProperty); }
            set { SetValue(IsDateTimeBetweenFilterControlProperty, value); }
        }

        public static readonly DependencyProperty IsDateTimeBetweenFilterControlProperty =
            DependencyProperty.Register("IsDateTimeBetweenFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsFirstFilterControl
        {
            get { return (bool)GetValue(IsFirstFilterControlProperty); }
            set { SetValue(IsFirstFilterControlProperty, value); }
        }

        public static readonly DependencyProperty IsFirstFilterControlProperty =
            DependencyProperty.Register("IsFirstFilterControl", typeof(bool), typeof(DataGridColumnFilter));

        public bool IsControlInitialized
        {
            get { return (bool)GetValue(IsControlInitializedProperty); }
            set { SetValue(IsControlInitializedProperty, value); }
        }

        public static readonly DependencyProperty IsControlInitializedProperty =
            DependencyProperty.Register("IsControlInitialized", typeof(bool), typeof(DataGridColumnFilter));
        #endregion

        #region Initialization
        private void initialize()
        {
            if (DataGridItemsSource != null && AssignedDataGridColumn != null && DataGrid != null)
            {
                initFilterData();

                initControlType();

                handleListFilterType();

                hookUpCommands();

                IsControlInitialized = true;
            }
        }

        private void initFilterData()
        {
            if (FilterCurrentData == null || !FilterCurrentData.IsTypeInitialized)
            {
                string valuePropertyBindingPath = getValuePropertyBindingPath(AssignedDataGridColumn);

                bool typeInitialized;

                Type valuePropertyType = getValuePropertyType(
                    valuePropertyBindingPath, getItemSourceElementType(out typeInitialized));

                FilterType filterType = getFilterType(
                    valuePropertyType,
                    isComboDataGridColumn(),
                    isBetweenType());

                FilterOperator filterOperator = FilterOperator.Undefined;

                string queryString = string.Empty;
                string queryStringTo = string.Empty;

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

        private void initControlType()
        {
            IsFirstFilterControl = false;

            IsTextFilterControl = false;
            IsNumericFilterControl = false;
            IsBooleanFilterControl = false;
            IsListFilterControl = false;
            IsDateTimeFilterControl = false;

            IsNumericBetweenFilterControl = false;
            IsDateTimeBetweenFilterControl = false;

            if (FilterType == FilterType.Text)
            {
                IsTextFilterControl = true;
            }
            else if (FilterType == FilterType.Numeric)
            {
                IsNumericFilterControl = true;
            }
            else if (FilterType == FilterType.Boolean)
            {
                IsBooleanFilterControl = true;
            }
            else if (FilterType == FilterType.List)
            {
                IsListFilterControl = true;
            }
            else if (FilterType == FilterType.DateTime)
            {
                IsDateTimeFilterControl = true;
            }
            else if (FilterType == FilterType.NumericBetween)
            {
                IsNumericBetweenFilterControl = true;
            }
            else if (FilterType == FilterType.DateTimeBetween)
            {
                IsDateTimeBetweenFilterControl = true;
            }
        }

        private void handleListFilterType()
        {
            if (FilterCurrentData.Type == FilterType.List)
            {
                ComboBox comboBox = Template.FindName("PART_ComboBoxFilter", this) as ComboBox;
                DataGridComboBoxColumn column = AssignedDataGridColumn as DataGridComboBoxColumn;

                if (comboBox != null && column != null)
                {
                    if (DataGridComboBoxExtensions.GetIsTextFilter(column))
                    {
                        FilterCurrentData.Type = FilterType.Text;
                        initControlType();
                    }
                    else //list filter type
                    {
                        Binding columnItemsSourceBinding = BindingOperations.GetBinding(column, DataGridComboBoxColumn.ItemsSourceProperty);

                        if (columnItemsSourceBinding == null)
                        {
                            Setter styleSetter = column.EditingElementStyle.Setters.First(s => ((Setter)s).Property == DataGridComboBoxColumn.ItemsSourceProperty) as Setter;
                            if (styleSetter != null)
                                columnItemsSourceBinding = styleSetter.Value as Binding;
                        }

                        comboBox.DisplayMemberPath = column.DisplayMemberPath;
                        comboBox.SelectedValuePath = column.SelectedValuePath;

                        if (columnItemsSourceBinding != null)
                        {
                            BindingOperations.SetBinding(comboBox, ItemsControl.ItemsSourceProperty, columnItemsSourceBinding);
                        }

                        comboBox.RequestBringIntoView
                            += new RequestBringIntoViewEventHandler(setComboBindingAndHanldeUnsetValue);
                    }
                }
            }
        }

        private void setComboBindingAndHanldeUnsetValue(object sender, RequestBringIntoViewEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            DataGridComboBoxColumn column = AssignedDataGridColumn as DataGridComboBoxColumn;

            if (column.ItemsSource == null)
            {
                if (combo.ItemsSource != null)
                {
                    IList list = combo.ItemsSource.Cast<object>().ToList();

                    if (list.Count > 0 && list[0] != DependencyProperty.UnsetValue)
                    {
                        combo.RequestBringIntoView -=
                            new RequestBringIntoViewEventHandler(setComboBindingAndHanldeUnsetValue);

                        list.Insert(0, DependencyProperty.UnsetValue);

                        combo.DisplayMemberPath = column.DisplayMemberPath;
                        combo.SelectedValuePath = column.SelectedValuePath;

                        combo.ItemsSource = list;
                    }
                }
            }
            else
            {
                combo.RequestBringIntoView -=
                    new RequestBringIntoViewEventHandler(setComboBindingAndHanldeUnsetValue);

                IList comboList = null;
                IList columnList = null;

                if (combo.ItemsSource != null)
                {
                    comboList = combo.ItemsSource.Cast<object>().ToList();
                }

                columnList = column.ItemsSource.Cast<object>().ToList();

                if (comboList == null
                    || columnList.Count > 0 && columnList.Count + 1 != comboList.Count)
                {
                    columnList = column.ItemsSource.Cast<object>().ToList();
                    columnList.Insert(0, DependencyProperty.UnsetValue);

                    combo.ItemsSource = columnList;
                }

                combo.RequestBringIntoView +=
                    new RequestBringIntoViewEventHandler(setComboBindingAndHanldeUnsetValue);
            }
        }

        private string getValuePropertyBindingPath(DataGridColumn column)
        {
            string path = string.Empty;

            if (column is DataGridBoundColumn)
            {
                DataGridBoundColumn bc = column as DataGridBoundColumn;
                path = (bc.Binding as Binding).Path.Path;
            }
            else if (column is DataGridTemplateColumn)
            {
                DataGridTemplateColumn tc = column as DataGridTemplateColumn;

                object templateContent = tc.CellTemplate.LoadContent();

                if (templateContent != null && templateContent is TextBlock)
                {
                    TextBlock block = templateContent as TextBlock;

                    BindingExpression binding = block.GetBindingExpression(TextBlock.TextProperty);

                    path = binding.ParentBinding.Path.Path;
                }
            }
            else if (column is DataGridComboBoxColumn)
            {
                DataGridComboBoxColumn comboColumn = column as DataGridComboBoxColumn;

                path = null;

                Binding binding = comboColumn.SelectedValueBinding as Binding ?? comboColumn.SelectedItemBinding as Binding ?? comboColumn.SelectedValueBinding as Binding;

                if (binding != null)
                {
                    path = binding.Path.Path;
                }

                if (comboColumn.SelectedItemBinding != null && comboColumn.SelectedValueBinding == null)
                {
                    if (path?.Trim().Length > 0)
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
            }

            return path;
        }

        private Type getValuePropertyType(string path, Type elementType)
        {
            Type type = typeof(object);

            if (elementType != null)
            {
                string[] properties = path.Split("."[0]);

                PropertyInfo pi = null;

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

        private Type getItemSourceElementType(out bool typeInitialized)
        {
            typeInitialized = false;

            Type elementType = null;

            IList l = DataGridItemsSource as IList;

            if (l?.Count > 0)
            {
                object obj = l[0];

                if (obj != null)
                {
                    elementType = l[0].GetType();
                    typeInitialized = true;
                }
                else
                {
                    elementType = typeof(object);
                }
            }
            if (l == null)
            {
                ListCollectionView lw = DataGridItemsSource as ListCollectionView;

                if (lw?.Count > 0)
                {
                    object obj = lw.CurrentItem;

                    if (obj != null)
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

        private FilterType getFilterType(
            Type valuePropertyType,
            bool isAssignedDataGridColumnComboDataGridColumn,
            bool isBetweenType)
        {
            FilterType filterType;

            if (isAssignedDataGridColumnComboDataGridColumn)
            {
                filterType = FilterType.List;
            }
            else if (valuePropertyType == typeof(bool) || valuePropertyType == typeof(bool?))
            {
                filterType = FilterType.Boolean;
            }
            else if (valuePropertyType == typeof(sbyte) || valuePropertyType == typeof(sbyte?))
            {
                filterType = FilterType.Numeric;
            }
            else if (valuePropertyType == typeof(byte) || valuePropertyType == typeof(byte?))
            {
                filterType = FilterType.Numeric;
            }
            else if (valuePropertyType == typeof(short) || valuePropertyType == typeof(short?))
            {
                filterType = FilterType.Numeric;
            }
            else if (valuePropertyType == typeof(ushort) || valuePropertyType == typeof(ushort?))
            {
                filterType = FilterType.Numeric;
            }
            else if (valuePropertyType == typeof(int) || valuePropertyType == typeof(int?))
            {
                filterType = FilterType.Numeric;
            }
            else if (valuePropertyType == typeof(uint) || valuePropertyType == typeof(uint?))
            {
                filterType = FilterType.Numeric;
            }
            else if (valuePropertyType == typeof(long) || valuePropertyType == typeof(long?))
            {
                filterType = FilterType.Numeric;
            }
            else if (valuePropertyType == typeof(float) || valuePropertyType == typeof(float?))
            {
                filterType = FilterType.Numeric;
            }
            else if (valuePropertyType == typeof(long) || valuePropertyType == typeof(long?))
            {
                filterType = FilterType.Numeric;
            }
            else if (valuePropertyType == typeof(decimal) || valuePropertyType == typeof(decimal?))
            {
                filterType = FilterType.Numeric;
            }
            else if (valuePropertyType == typeof(float) || valuePropertyType == typeof(float?))
            {
                filterType = FilterType.Numeric;
            }
            else if (valuePropertyType == typeof(double) || valuePropertyType == typeof(double?))
            {
                filterType = FilterType.Numeric;
            }
            else if (valuePropertyType == typeof(long) || valuePropertyType == typeof(long?))
            {
                filterType = FilterType.Numeric;
            }
            else if (valuePropertyType == typeof(DateTime) || valuePropertyType == typeof(DateTime?))
            {
                filterType = FilterType.DateTime;
            }
            else
            {
                filterType = FilterType.Text;
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

        private bool isComboDataGridColumn() => AssignedDataGridColumn is DataGridComboBoxColumn;

        private bool isBetweenType() => DataGridColumnExtensions.GetIsBetweenFilterControl(AssignedDataGridColumn);

        private void hookUpCommands()
        {
            if (DataGridExtensions.GetClearFilterCommand(DataGrid) == null)
            {
                DataGridExtensions.SetClearFilterCommand(
                    DataGrid, new DataGridFilterCommand(clearQuery));
            }
        }

        private
        #endregion

        #region Querying
        void filterCurrentData_FilterChangedEvent(object sender, EventArgs e)
        {
            if (DataGrid != null)
            {
                QueryController query = QueryControllerFactory.GetQueryController(
                    DataGrid, FilterCurrentData, DataGridItemsSource);

                addFilterStateHandlers(query);

                query.DoQuery();

                IsFirstFilterControl = query.IsCurentControlFirstControl;
            }
        }

        private void clearQuery(object parameter)
        {
            if (DataGrid != null)
            {
                QueryController query = QueryControllerFactory.GetQueryController(
                    DataGrid, FilterCurrentData, DataGridItemsSource);

                query.ClearFilter();
            }
        }

        private void addFilterStateHandlers(QueryController query)
        {
            query.FilteringStarted -= query_FilteringStarted;
            query.FilteringFinished -= query_FilteringFinished;

            query.FilteringStarted += query_FilteringStarted;
            query.FilteringFinished += query_FilteringFinished;
        }

        private void query_FilteringFinished(object sender, EventArgs e)
        {
            if (FilterCurrentData != null && FilterCurrentData.Equals((sender as QueryController).ColumnFilterData))
            {
                IsFilteringInProgress = false;
            }
        }

        private void query_FilteringStarted(object sender, EventArgs e)
        {
            if (FilterCurrentData != null && FilterCurrentData.Equals((sender as QueryController).ColumnFilterData))
            {
                IsFilteringInProgress = true;
            }
        }
        #endregion
    }
}