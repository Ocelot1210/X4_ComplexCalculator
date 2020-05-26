using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;


namespace X4_ComplexCalculator.Main.PlanningArea.UI.StorageAssign
{
    public class StorageAssignGroupValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is CollectionViewGroup cvg))
            {
                return value;
            }

            var target = cvg.Items.Cast<StorageAssignGridItem>().Where(x => x.TransportTypeID == (string)cvg.Name).First();

            //return $"{target.TransportTypeName} [{target.CapacityInfo.UsedCapacity}/{target.CapacityInfo.TotalCapacity}]";
            return target.TransportTypeName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
