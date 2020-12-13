﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support
{
    public class ClearFilterButtonVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)values[0] && (bool)values[1])
            {
                return System.Windows.Visibility.Visible;
            }
            else
            {
                return System.Windows.Visibility.Collapsed;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
