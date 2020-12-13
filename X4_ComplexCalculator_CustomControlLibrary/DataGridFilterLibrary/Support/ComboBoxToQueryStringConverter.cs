using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support
{
    public class ComboBoxToQueryStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => value?.ToString() == String.Empty ? null : value;

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => value;

        #endregion
    }
}
