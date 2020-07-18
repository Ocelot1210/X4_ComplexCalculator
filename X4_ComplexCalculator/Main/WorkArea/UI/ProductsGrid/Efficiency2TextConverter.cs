using System;
using System.Globalization;
using System.Windows.Data;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// 生産性を表示文字列に変換するValueConverter
    /// </summary>
    class Efficiency2TextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double val)
            {
                return (val < 0) ? "-" : $"{(int)(val * 100)}%";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
