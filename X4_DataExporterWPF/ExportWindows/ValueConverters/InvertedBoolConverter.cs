using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace X4_DataExporterWPF.ExportWindow.ValueConverters;


/// <summary>
/// <see cref="bool"/> 値を反転する <see cref="IValueConverter"/>
/// </summary>
internal sealed class InvertedBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not bool ret)
        {
            return DependencyProperty.UnsetValue;
        }

        return !ret;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not bool ret)
        {
            return DependencyProperty.UnsetValue;
        }

        return !ret;
    }
}