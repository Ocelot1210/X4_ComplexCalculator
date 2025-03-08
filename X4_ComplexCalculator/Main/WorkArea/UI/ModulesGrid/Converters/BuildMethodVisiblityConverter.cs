using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.Converters;


/// <summary>
/// 建造方式のコンボボックス表示状態用 ValueConverter
/// </summary>
public sealed class BuildMethodVisiblityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IX4Module module)
        {
            // 建造方式の種類が2つ以上の場合、コンボボックスを表示して選択可能にする
            return (2 <= module.Productions.Count) ? Visibility.Visible : Visibility.Hidden;
        }

        return Binding.DoNothing;
    }


    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}