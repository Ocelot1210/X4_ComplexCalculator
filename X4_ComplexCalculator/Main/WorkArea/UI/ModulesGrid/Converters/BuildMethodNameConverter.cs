using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.Converters;

/// <summary>
/// 建造方式名を表示するための <see cref="IValueConverter"/>
/// </summary>
public sealed class BuildMethodNameConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value as IWareProduction)?.Name ?? Binding.DoNothing;
    }


    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
