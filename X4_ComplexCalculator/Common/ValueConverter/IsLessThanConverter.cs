using System;
using System.Globalization;
using System.Windows.Data;

namespace X4_ComplexCalculator.Common.ValueConverter
{
    /// <summary>
    /// 特定の値未満なら動作するValueConverter
    /// </summary>
    public class IsLessThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string param)
            {
                throw new ArgumentException($"paran ${parameter} must be string.", nameof(parameter));
            }
            return System.Convert.ToInt64(value) < long.Parse(param);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
