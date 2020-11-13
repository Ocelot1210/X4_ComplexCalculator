using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace X4_ComplexCalculator.Common.ValueConverter
{
    public class FormatStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 1 || values[0] is not string key)
            {
                throw new ArgumentException("The first parameter must be language key.", nameof(values));
            }

            return string.Format(key, values.Skip(1).ToArray());
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
