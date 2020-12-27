using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace X4_ComplexCalculator.Common.ValueConverter
{
    /// <summary>
    /// 列挙型とbool型を変換するValueConverter
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string paramStr)
            {
                return DependencyProperty.UnsetValue;
            }

            if (!Enum.IsDefined(value.GetType(), value))
            {
                return DependencyProperty.UnsetValue;
            }

            var paramValue = Enum.Parse(value.GetType(), paramStr);

            return paramValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string paramStr)
            {
                return DependencyProperty.UnsetValue;
            }

            return true.Equals(value) ? Enum.Parse(targetType, paramStr) : DependencyProperty.UnsetValue;
        }
    }
}
