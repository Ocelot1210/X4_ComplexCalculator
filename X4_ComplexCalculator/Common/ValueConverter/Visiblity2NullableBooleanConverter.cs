using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace X4_ComplexCalculator.Common.ValueConverter
{
    /// <summary>
    /// Visiblityとboolの変換を行う
    /// </summary>
    class Visiblity2NullableBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type _, object parameter, CultureInfo culture)
        {
            var ret = (value is Visibility) ? ((Visibility)value) == Visibility.Visible : Binding.DoNothing;
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ret = (value is bool?)? (((bool?)value) == true)? Visibility.Visible : Visibility.Collapsed :
                      (value is bool)?  (((bool?)value) == true)? Visibility.Visible : Visibility.Collapsed : Binding.DoNothing;

            return ret;
        }
    }
}
