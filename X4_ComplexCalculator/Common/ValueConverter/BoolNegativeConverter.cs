using System;
using System.Globalization;
using System.Windows.Data;

namespace X4_ComplexCalculator.Common.ValueConverter;

/// <summary>
/// bool値を反転するValueConverter
/// </summary>
public class BoolNegativeConverter : IValueConverter
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
