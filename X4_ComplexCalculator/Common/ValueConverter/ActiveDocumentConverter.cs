using System;
using System.Windows.Data;
using X4_ComplexCalculator.Main.WorkArea;

namespace X4_ComplexCalculator.Common.ValueConverter
{
    /// <summary>
    /// 
    /// </summary>
    public class ActiveDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is WorkAreaViewModel)
            {
                return value;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is WorkAreaViewModel)
            {
                return value;
            }

            return Binding.DoNothing;
        }
    }
}
