using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace X4_ComplexCalculator.Common.ValueConverters;


/// <summary>
/// 作業エリアのタイトル文字列用ValueConverter
/// </summary>
/// <remarks>
/// 何らかの変更があれば、タイトル文字列の末尾に '*' を追加する
/// </remarks>
public sealed class WorkAreaTitleValueConverter : IMultiValueConverter
{
    /// <inheritdoc/>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
        {
            return DependencyProperty.UnsetValue;
        }

        if (values[0] is not string title)
        {
            return DependencyProperty.UnsetValue;
        }

        if (values[1] is not bool hasChanged)
        {
            return DependencyProperty.UnsetValue;
        }

        return hasChanged ? $"{title}*" : title;
    }


    /// <inheritdoc/>
    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
    {
        if (value is not string title)
        {
            return [DependencyProperty.UnsetValue];
        }

        var hasChanged = title[^1] == '*';

        return [hasChanged ? title[..^1] : hasChanged, hasChanged];
    }
}
