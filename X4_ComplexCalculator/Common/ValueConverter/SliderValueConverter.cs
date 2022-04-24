using System;
using System.Windows.Data;

namespace X4_ComplexCalculator.Common.ValueConverter;

/// <summary>
/// DataGridのヘッダ部分のスライダー用ValueConverter
/// </summary>
public class SliderValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is double)
        {
            return value;
        }

        return parameter ?? Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is double)
        {
            return value;
        }

        return parameter ?? Binding.DoNothing;
    }
}
