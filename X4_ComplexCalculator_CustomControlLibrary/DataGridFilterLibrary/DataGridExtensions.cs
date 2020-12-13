﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Querying;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary
{
    public class DataGridExtensions
    {
        public static DependencyProperty DataGridFilterQueryControllerProperty =
            DependencyProperty.RegisterAttached("DataGridFilterQueryController",
                typeof(QueryController), typeof(DataGridExtensions));

        public static QueryController GetDataGridFilterQueryController(DependencyObject target) => (QueryController)target.GetValue(DataGridFilterQueryControllerProperty);

        public static void SetDataGridFilterQueryController(DependencyObject target, QueryController value)
        {
            target.SetValue(DataGridFilterQueryControllerProperty, value);
        }

        public static DependencyProperty ClearFilterCommandProperty =
            DependencyProperty.RegisterAttached("ClearFilterCommand",
                typeof(DataGridFilterCommand), typeof(DataGridExtensions));

        public static DataGridFilterCommand GetClearFilterCommand(DependencyObject target) => (DataGridFilterCommand)target.GetValue(ClearFilterCommandProperty);

        public static void SetClearFilterCommand(DependencyObject target, DataGridFilterCommand value)
        {
            target.SetValue(ClearFilterCommandProperty, value);
        }

        public static DependencyProperty IsFilterVisibleProperty =
            DependencyProperty.RegisterAttached("IsFilterVisible",
                typeof(bool?), typeof(DataGridExtensions),
                  new FrameworkPropertyMetadata(true));

        public static bool? GetIsFilterVisible(
DependencyObject target) => (bool)target.GetValue(IsFilterVisibleProperty);

        public static void SetIsFilterVisible(
            DependencyObject target, bool? value)
        {
            target.SetValue(IsFilterVisibleProperty, value);
        }

        public static DependencyProperty UseBackgroundWorkerForFilteringProperty =
            DependencyProperty.RegisterAttached("UseBackgroundWorkerForFiltering",
                typeof(bool), typeof(DataGridExtensions),
                  new FrameworkPropertyMetadata(false));

        public static bool GetUseBackgroundWorkerForFiltering(
DependencyObject target) => (bool)target.GetValue(UseBackgroundWorkerForFilteringProperty);

        public static void SetUseBackgroundWorkerForFiltering(
            DependencyObject target, bool value)
        {
            target.SetValue(UseBackgroundWorkerForFilteringProperty, value);
        }

        public static DependencyProperty IsClearButtonVisibleProperty =
            DependencyProperty.RegisterAttached("IsClearButtonVisible",
                typeof(bool), typeof(DataGridExtensions),
                  new FrameworkPropertyMetadata(true));

        public static bool GetIsClearButtonVisible(
DependencyObject target) => (bool)target.GetValue(IsClearButtonVisibleProperty);

        public static void SetIsClearButtonVisible(
            DependencyObject target, bool value)
        {
            target.SetValue(IsClearButtonVisibleProperty, value);
        }
    }
}
