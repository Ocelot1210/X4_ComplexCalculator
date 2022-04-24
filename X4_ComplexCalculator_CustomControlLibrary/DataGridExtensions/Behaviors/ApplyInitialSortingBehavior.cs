﻿namespace X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions.Behaviors;

using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Throttle;
using X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions.Framework;

/// <summary>
/// A behavior to control the "ApplyInitialSorting" feature.
/// </summary>
public class ApplyInitialSortingBehavior : Behavior<DataGrid>
{
    private IList<KeyValuePair<string, ListSortDirection>>? _mostRecentDescriptions;
    private IList<KeyValuePair<string, ListSortDirection>> _lastKnownActiveDescriptions = new List<KeyValuePair<string, ListSortDirection>>();

    /// <summary>
    /// Called after the behavior is attached to an AssociatedObject.
    /// </summary>
    /// <remarks>
    /// Override this to hook up functionality to the AssociatedObject.
    /// </remarks>
    protected override void OnAttached()
    {
        base.OnAttached();

        var dataGrid = AssociatedObject;

        ((INotifyCollectionChanged)dataGrid.Items.SortDescriptions).CollectionChanged += SortDescriptions_CollectionChanged;
        DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid)).AddValueChanged(dataGrid, ItemsSource_Changed);
    }

    /// <summary>
    /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
    /// </summary>
    /// <remarks>
    /// Override this to unhook functionality from the AssociatedObject.
    /// </remarks>
    protected override void OnDetaching()
    {
        base.OnDetaching();

        var dataGrid = AssociatedObject;

        ((INotifyCollectionChanged)dataGrid.Items.SortDescriptions).CollectionChanged -= SortDescriptions_CollectionChanged;
        DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid)).RemoveValueChanged(dataGrid, ItemsSource_Changed);
    }

    private void SortDescriptions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var dataGrid = AssociatedObject;

        _mostRecentDescriptions = dataGrid.Items.SortDescriptions
            .Skip(dataGrid.Items.GroupDescriptions.Count)
            .Select(s => new KeyValuePair<string, ListSortDirection>(s.PropertyName, s.Direction))
            .ToList();

        PersistLastKnownSortDescriptions();
    }

    private void ItemsSource_Changed(object? sender, EventArgs e)
    {
        try
        {
            var dataGrid = AssociatedObject;
            var dataGridItems = dataGrid.Items;

            if (_lastKnownActiveDescriptions.Any() && !dataGridItems.SortDescriptions.Any())
            {
                foreach (var item in _lastKnownActiveDescriptions)
                {
                    var column = dataGrid.Columns.FirstOrDefault(col => col.SortMemberPath == item.Key);
                    if (column is not null)
                    {
                        column.SortDirection = item.Value;
                    }
                }
            }

            var groupDescriptions = dataGridItems.GroupDescriptions;

            dataGridItems.SortDescriptions.Clear();

            foreach (var groupDescription in groupDescriptions.OfType<PropertyGroupDescription>())
            {
                dataGridItems.SortDescriptions.Add(new SortDescription(groupDescription.PropertyName, ListSortDirection.Ascending));
            }

            foreach (var column in dataGrid.Columns.Where(c => c?.SortDirection is not null && !string.IsNullOrEmpty(c.SortMemberPath)))
            {
                dataGridItems.SortDescriptions.Add(new SortDescription(column.SortMemberPath, column.SortDirection.GetValueOrDefault()));
            }
        }
        catch (InvalidOperationException)
        {
            // in some special cases we may get:
            // System.InvalidOperationException: 'Sorting' is not allowed during an AddNew or EditItem transaction.
            // => just ignore, there is nothing we can do about it...
        }
    }

    [Throttled(typeof(DispatcherThrottle), (int)DispatcherPriority.Normal)]
    private void PersistLastKnownSortDescriptions()
    {
        _lastKnownActiveDescriptions = _mostRecentDescriptions ?? _lastKnownActiveDescriptions;
    }

    internal static void IsEnabled_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not DataGrid dataGrid)
            return;

        var behaviors = Interaction.GetBehaviors(dataGrid);

        var existing = behaviors.FirstOrDefault(b => b is ApplyInitialSortingBehavior);

        if (true.Equals(e.NewValue))
        {
            if (existing is null)
            {
                behaviors.Add(new ApplyInitialSortingBehavior());
            }
        }
        else
        {
            behaviors.Remove(existing);
        }
    }
}
