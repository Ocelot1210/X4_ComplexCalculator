using System;
using System.Windows.Data;
using System.Globalization;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support
{
    public class DatePickerToQueryStringConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value?.ToString()))
            {
                return null;
            }

            if (DateTime.TryParse(
                value.ToString(),
                culture.DateTimeFormat,
                DateTimeStyles.None,
                out DateTime dateTime))
            {
                return dateTime;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
