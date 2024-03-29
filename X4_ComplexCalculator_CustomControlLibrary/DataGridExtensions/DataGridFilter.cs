﻿namespace X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Defines the attached properties that can be set on the data grid level.
/// </summary>
public static class DataGridFilter
{
    #region IsAutoFilterEnabled attached property

    /// <summary>
    /// Gets if the default filters are automatically attached to each column.
    /// </summary>
    /// <param name="dataGrid">The data grid.</param>
    /// <returns><c>true</c> is auto-filtering is enabled.</returns>
    [AttachedPropertyBrowsableForType(typeof(DataGrid))]
    public static bool GetIsAutoFilterEnabled(DependencyObject dataGrid)
    {
        return (bool)dataGrid.GetValue(IsAutoFilterEnabledProperty);
    }
    /// <summary>
    /// Sets if the default filters are automatically attached to each column. Set to false if you want to control filters by code.
    /// </summary>
    /// <param name="dataGrid">The data grid.</param>
    /// <param name="value">The value.</param>
    public static void SetIsAutoFilterEnabled(DependencyObject dataGrid, bool value)
    {
        dataGrid.SetValue(IsAutoFilterEnabledProperty, value);
    }
    /// <summary>
    /// Identifies the IsAutoFilterEnabled dependency property
    /// </summary>
    public static readonly DependencyProperty IsAutoFilterEnabledProperty =
        DependencyProperty.RegisterAttached("IsAutoFilterEnabled", typeof(bool), typeof(DataGridFilter), new FrameworkPropertyMetadata(false, IsAutoFilterEnabled_Changed));

    private static void IsAutoFilterEnabled_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var dataGrid = sender as DataGrid;
        // Force creation of the host and show or hide the controls.
        dataGrid?.GetFilter().Enable(true.Equals(e.NewValue));
    }

    #endregion

    #region Filter attached property

    /// <summary>
    /// Filter attached property to attach the DataGridFilterHost instance to the owning DataGrid.
    /// This property is only used by code and is not accessible from XAML.
    /// </summary>
    /// <param name="dataGrid">The data grid.</param>
    /// <returns>The <see cref="DataGridFilterHost"/></returns>
    public static DataGridFilterHost GetFilter(this DataGrid dataGrid)
    {
        var value = (DataGridFilterHost)dataGrid.GetValue(_FilterProperty);
        if (value is null)
        {
            value = new DataGridFilterHost(dataGrid);
            dataGrid.SetValue(_FilterProperty, value);
        }
        return value;
    }
    /// <summary>
    /// Identifies the Filters dependency property.
    /// This property definition is private, so it's only accessible by code and can't be messed up by invalid bindings.
    /// </summary>
    private static readonly DependencyProperty _FilterProperty =
        DependencyProperty.RegisterAttached("Filter", typeof(DataGridFilterHost), typeof(DataGridFilter));

    #endregion

    #region ContentFilterFactory attached property

    private static readonly IContentFilterFactory _DefaultContentFilterFactory = new SimpleContentFilterFactory(StringComparison.CurrentCultureIgnoreCase);

    /// <summary>
    /// Gets the content filter factory for the data grid filter.
    /// </summary>
    /// <param name="dataGrid">The data grid.</param>
    /// <returns>The <see cref="IContentFilterFactory"/></returns>
    [AttachedPropertyBrowsableForType(typeof(DataGrid))]
    public static IContentFilterFactory GetContentFilterFactory(DependencyObject dataGrid)
    {
        return (IContentFilterFactory)dataGrid.GetValue(ContentFilterFactoryProperty) ?? _DefaultContentFilterFactory;
    }
    /// <summary>
    /// Sets the content filter factory for the data grid filter.
    /// </summary>
    /// <param name="dataGrid">The data grid.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentNullException">dataGrid</exception>
    public static void SetContentFilterFactory(DependencyObject dataGrid, IContentFilterFactory? value)
    {
        if (dataGrid is null)
            throw new ArgumentNullException(nameof(dataGrid));

        dataGrid.SetValue(ContentFilterFactoryProperty, value);
    }
    /// <summary>
    /// Identifies the ContentFilterFactory dependency property
    /// </summary>
    public static readonly DependencyProperty ContentFilterFactoryProperty =
        DependencyProperty.RegisterAttached("ContentFilterFactory", typeof(IContentFilterFactory), typeof(DataGridFilter), new FrameworkPropertyMetadata(_DefaultContentFilterFactory, null, ContentFilterFactory_CoerceValue));

    private static object ContentFilterFactory_CoerceValue(DependencyObject sender, object? value)
    {
        // Ensure non-null content filter.
        return value ?? _DefaultContentFilterFactory;
    }

    #endregion

    #region Delay of the filter evaluation throttle.

    /// <summary>
    /// Gets the delay that is used to throttle filter changes before the filter is applied.
    /// </summary>
    /// <param name="dataGrid">The data grid</param>
    /// <returns>The throttle delay.</returns>
    public static TimeSpan GetFilterEvaluationDelay(DependencyObject dataGrid)
    {
        return (TimeSpan)dataGrid.GetValue(FilterEvaluationDelayProperty);
    }

    /// <summary>
    /// Sets the delay that is used to throttle filter changes before the filter is applied.
    /// </summary>
    /// <param name="dataGrid">The data grid</param>
    /// <param name="value">The new throttle delay.</param>
    public static void SetFilterEvaluationDelay(DependencyObject dataGrid, TimeSpan value)
    {
        dataGrid.SetValue(FilterEvaluationDelayProperty, value);
    }
    /// <summary>
    /// Identifies the FilterEvaluationDelay dependency property
    /// </summary>
    public static readonly DependencyProperty FilterEvaluationDelayProperty =
        DependencyProperty.RegisterAttached("FilterEvaluationDelay", typeof(TimeSpan), typeof(DataGridFilter), new FrameworkPropertyMetadata(TimeSpan.FromSeconds(0.5)));

    #endregion

    /// <summary>
    /// Gets the resource locator.
    /// </summary>
    /// <param name="dataGrid">The object.</param>
    /// <returns>The locator</returns>
    [AttachedPropertyBrowsableForType(typeof(DataGrid))]
    public static IResourceLocator? GetResourceLocator(DependencyObject dataGrid)
    {
        return (IResourceLocator)dataGrid.GetValue(ResourceLocatorProperty);
    }
    /// <summary>
    /// Sets the resource locator.
    /// </summary>
    /// <param name="dataGrid">The object.</param>
    /// <param name="value">The value.</param>
    public static void SetResourceLocator(DependencyObject dataGrid, IResourceLocator? value)
    {
        dataGrid.SetValue(ResourceLocatorProperty, value);
    }
    /// <summary>
    /// Identifies the <see cref="P:DataGridExtensions.DataGridFilter.ResourceLocator"/> attached property
    /// </summary>
    /// <AttachedPropertyComments>
    /// <summary>
    /// Set an resource locator to locate resource if the component resource keys can not be found, e.g. because dgx is used in a plugin and multiple assemblies with resources might exist.
    /// </summary>
    /// </AttachedPropertyComments>
    public static readonly DependencyProperty ResourceLocatorProperty =
        DependencyProperty.RegisterAttached("ResourceLocator", typeof(IResourceLocator), typeof(DataGridFilter), new FrameworkPropertyMetadata(null));


    /// <summary>
    /// Gets the value of the <see cref="P:DataGridExtensions.DataGridFilter.GlobalFilter"/> attached property from a given <see cref="DataGrid"/>.
    /// </summary>
    /// <param name="dataGrid">The <see cref="DataGrid"/> from which to read the property value.</param>
    /// <returns>the value of the <see cref="P:DataGridExtensions.DataGridFilter.GlobalFilter"/> attached property.</returns>
    [AttachedPropertyBrowsableForType(typeof(DataGrid))]
    public static Predicate<object?>? GetGlobalFilter(DependencyObject dataGrid)
    {
        return (Predicate<object?>?)dataGrid.GetValue(GlobalFilterProperty);
    }
    /// <summary>
    /// Sets the value of the <see cref="P:DataGridExtensions.DataGridFilter.GlobalFilter" /> attached property to a given <see cref="DataGrid" />.
    /// </summary>
    /// <param name="dataGrid">The <see cref="DataGrid" /> on which to set the property value.</param>
    /// <param name="value">The property value to set.</param>
    public static void SetGlobalFilter(DependencyObject dataGrid, Predicate<object?>? value)
    {
        dataGrid.SetValue(GlobalFilterProperty, value);
    }
    /// <summary>
    /// Identifies the <see cref="P:DataGridExtensions.DataGridFilter.GlobalFilter"/> dependency property.
    /// </summary>
    /// <AttachedPropertyComments>
    /// <summary>
    /// Allows to specify a global filter that is applied to the items in addition to the column filters.
    /// </summary>
    /// </AttachedPropertyComments>
    public static readonly DependencyProperty GlobalFilterProperty =
        DependencyProperty.RegisterAttached("GlobalFilter", typeof(Predicate<object>), typeof(DataGridFilter), new FrameworkPropertyMetadata(GlobalFilter_Changed));

    private static void GlobalFilter_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DataGrid)d).GetFilter().SetGlobalFilter((Predicate<object?>)e.NewValue);
    }


    #region 仮想化対策
    /// <summary>
    /// フィルターを管理するディクショナリ
    /// </summary>
    private static readonly DependencyProperty _FilterManagerProperty =
        DependencyProperty.RegisterAttached("FilterManager", typeof(IDictionary<string, IContentFilter>), typeof(DataGridFilter), new FrameworkPropertyMetadata(null));

    private static IDictionary<string, IContentFilter> GetFilterManager(DependencyObject dataGrid)
    {
        var ret = (IDictionary<string, IContentFilter>?)dataGrid.GetValue(_FilterManagerProperty);
        if (ret is null)
        {
            ret = new Dictionary<string, IContentFilter>();
            dataGrid.SetValue(_FilterManagerProperty, ret);
        }
        return ret;
    }


    public static IContentFilter? LoadFilter(this DataGrid dataGrid, DataGridColumn column)
    {
        var fm = GetFilterManager(dataGrid);
        if (fm.TryGetValue(column.SortMemberPath, out var contentFilter))
        {
            return contentFilter;
        }

        return null;
    }

    public static void SaveFilter(this DataGrid dataGrid, DataGridColumn column)
    {
        var filter = DataGridFilterColumn.GetActiveFilter(column);
        if (filter is null)
        {
            return;
        }

        var key = column.SortMemberPath;
        var fm = GetFilterManager(dataGrid);
        fm[key] = filter;
    }
    #endregion


    #region Resource keys

    /// <summary>
    /// Template for the filter on a column represented by a DataGridTextColumn.
    /// </summary>
    public static readonly ResourceKey TextColumnFilterTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridTextColumn));

    /// <summary>
    /// Template for the filter on a column represented by a DataGridCheckBoxColumn.
    /// </summary>
    public static readonly ResourceKey CheckBoxColumnFilterTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridCheckBoxColumn));

    /// <summary>
    /// Template for the filter on a column represented by a DataGridCheckBoxColumn.
    /// </summary>
    public static readonly ResourceKey TemplateColumnFilterTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridTemplateColumn));

    /// <summary>
    /// Template for the filter on a column represented by a DataGridComboBoxColumn.
    /// </summary>
    public static readonly ResourceKey ComboBoxColumnFilterTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridComboBoxColumn));

    /// <summary>
    /// Template for the whole column header.
    /// </summary>
    public static readonly ResourceKey ColumnHeaderTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderTemplate");

    /// <summary>
    /// The filter icon template.
    /// </summary>
    public static readonly ResourceKey IconTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), "IconTemplate");

    /// <summary>
    /// The filter icon style.
    /// </summary>
    public static readonly ResourceKey IconStyleKey = new ComponentResourceKey(typeof(DataGridFilter), "IconStyle");

    /// <summary>
    /// Style for the filter check box in a filtered DataGridCheckBoxColumn.
    /// </summary>
    public static readonly ResourceKey ColumnHeaderSearchCheckBoxStyleKey = new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderSearchCheckBoxStyle");

    /// <summary>
    /// Style for the filter text box in a filtered DataGridTextColumn.
    /// </summary>
    public static readonly ResourceKey ColumnHeaderSearchTextBoxStyleKey = new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderSearchTextBoxStyle");

    /// <summary>
    /// Style for the clear button in the filter text box in a filtered DataGridTextColumn.
    /// </summary>
    public static readonly ResourceKey ColumnHeaderSearchTextBoxClearButtonStyleKey = new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderSearchTextBoxClearButtonStyle");

    #endregion
}
