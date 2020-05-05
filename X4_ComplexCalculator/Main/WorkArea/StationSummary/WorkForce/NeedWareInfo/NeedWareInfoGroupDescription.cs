using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace X4_ComplexCalculator.Main.WorkArea.StationSummary.WorkForce.NeedWareInfo
{
    /// <summary>
    /// 必要ウェアグループ化用
    /// </summary>
    class NeedWareInfoGroupDescription : PropertyGroupDescription
    {
        public override object GroupNameFromItem(object item, int level, CultureInfo culture)
        {
            var obj = (NeedWareInfoDetailsItem)item;

            return obj.Method;
        }
    }
}
