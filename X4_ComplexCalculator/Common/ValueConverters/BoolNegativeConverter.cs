using System;
using System.Globalization;
using System.Windows.Data;

namespace X4_ComplexCalculator.Common.ValueConverters;

/// <summary>
/// bool値を反転するValueConverter
/// </summary>
public sealed class BoolNegativeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(value is bool boolean && boolean);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(value is bool boolean && boolean);
    }
}
