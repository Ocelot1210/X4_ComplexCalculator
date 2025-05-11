using System;
using System.Windows.Data;

namespace X4_ComplexCalculator.Common.ValueConverters;


public sealed class LongUpDownValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        long? tmp = value switch
        {
            short  => (short)value,
            int    => (int)value,
            long   => (long)value,
            float  => (long)(float)value,
            double => (long)(double)value,
            _      => null
        };

        if (tmp.HasValue)
        {
            return tmp.Value;
        }

        if (parameter is not null && long.TryParse(parameter.ToString(), out var result))
        {
            return result;
        }
        
        return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        long? tmp = value switch
        {
            short  => (short)value,
            int    => (int)value,
            long   => (long)value,
            float  => (long)(float)value,
            double => (long)(double)value,
            _      => null
        };

        if (tmp.HasValue)
        {
            return tmp.Value;
        }

        if (parameter is not null && long.TryParse(parameter.ToString(), out var result))
        {
            return result;
        }

        return Binding.DoNothing;
    }
}

public sealed class DoubleUpDownValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        double? tmp = value switch
        {
            short  => (short)value,
            int    => (int)value,
            long   => (long)value,
            float  => (float)value,
            double => (double)value,
            _      => null
        };

        if (tmp.HasValue)
        {
            return tmp.Value;
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