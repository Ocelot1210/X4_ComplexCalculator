using System;
using System.Windows.Data;

namespace X4_ComplexCalculator.Common.ValueConverter;


public class LongUpDownValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is long)
        {
            return value;
        }

        if (parameter is not null && long.TryParse(parameter.ToString(), out var result))
        {
            return result;
        }
        
        return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is long)
        {
            return value;
        }

        if (parameter is not null && long.TryParse(parameter.ToString(), out var result))
        {
            return result;
        }

        return Binding.DoNothing;
    }
}

public class DoubleUpDownValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is double)
        {
            return value;
        }

        if (parameter is not null && double.TryParse(parameter.ToString(), out var result))
        {
            return result;
        }

        return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is double)
        {
            return value;
        }

        if (parameter is not null && double.TryParse(parameter.ToString(), out var result))
        {
            return result;
        }

        return Binding.DoNothing;
    }
}