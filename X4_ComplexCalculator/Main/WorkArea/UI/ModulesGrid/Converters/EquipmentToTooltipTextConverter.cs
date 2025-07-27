using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ZLinq;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.Converters;

/// <summary>
/// ウェアの装備品管理情報と装備種別IDをツールチップ文字列に変換するクラス
/// </summary>
sealed internal class EquipmentToTooltipTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EquippableWareEquipmentManager manager && parameter is string equipmentTypeID)
        {
            return MakeDetailText(manager, equipmentTypeID);
        }

        return "";
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// ツールチップ用の文字列を作成
    /// </summary>
    private static string MakeDetailText(EquippableWareEquipmentManager manager, string equipmentTypeID)
    {
        var equipments = manager.AllEquipments
            .AsValueEnumerable()
            .Where(x => x.EquipmentType.EquipmentTypeID == equipmentTypeID);

        // 装備が無い場合は専用のテキストを表示
        if (!equipments.Any())
        {
            return (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("Lang:Common_NotEquippedToolTipText", null, null);
        }

        var sb = new StringBuilder(256);

        var groups = equipments.GroupBy(x => x.Size)
            .OrderByDescending(x => x.Key is not null)
            .ThenBy(x => x.Key);

        foreach (var group in groups)
        {
            var cnt = 1;

            foreach (var ware in group)
            {
                if (cnt == 1)
                {
                    // サイズが切り替わった直後なら改行する
                    if (sb.Length != 0)
                    {
                        sb.AppendLine();
                    }
                    sb.AppendLine($"【{group.Key?.Name ?? ""}】");
                }
                sb.AppendLine($"{cnt++:D2} : {ware.Name}");
            }
        }

        // 最後の改行を消す
        sb.Length -= 2;

        return sb.ToString();
    }
}
