using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using X4_ComplexCalculator.Entities;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.Converters;


/// <summary>
/// 装備編集関連コントロールの表示状態用 ValueConverter
/// </summary>
public sealed class EditEquipmentButtonVisiblityConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EquippableWareEquipmentManager mgr)
        {
            // 装備スロットを持つモジュールの場合、装備を編集可能にする
            return (mgr.CanEquipped) ? Visibility.Visible : Visibility.Hidden;
        }

        return Binding.DoNothing;
    }


    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
