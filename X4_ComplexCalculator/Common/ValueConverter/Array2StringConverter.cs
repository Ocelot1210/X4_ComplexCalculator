using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace X4_ComplexCalculator.Common.ValueConverter;

/// <summary>
/// 配列を指定した区切り文字で分割した文字列に変換するValueConverter
/// </summary>
public class Array2StringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IEnumerable arr)
        {
            return string.Join(parameter.ToString(), ConvertSub(arr));
        }

        return value?.ToString() ?? "";
    }

    private IEnumerable<string> ConvertSub(IEnumerable enumerable)
    {
        foreach (var item in enumerable)
        {
            yield return item?.ToString() ?? "";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
