using System.Windows;
using System.Windows.Controls;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary
{
    public class DataGridColumnExtensions
    {
        public static DependencyProperty IsCaseSensitiveSearchProperty =
            DependencyProperty.RegisterAttached("IsCaseSensitiveSearch", typeof(bool), typeof(DataGridColumn));

        public static bool GetIsCaseSensitiveSearch(DependencyObject target)
            => (bool)target.GetValue(IsCaseSensitiveSearchProperty);

        public static void SetIsCaseSensitiveSearch(DependencyObject target, bool value)
            => target.SetValue(IsCaseSensitiveSearchProperty, value);



        public static DependencyProperty IsBetweenFilterControlProperty =
            DependencyProperty.RegisterAttached("IsBetweenFilterControl", typeof(bool), typeof(DataGridColumn));

        public static bool GetIsBetweenFilterControl(DependencyObject target)
            => (bool)target.GetValue(IsBetweenFilterControlProperty);

        public static void SetIsBetweenFilterControl(DependencyObject target, bool value)
            => target.SetValue(IsBetweenFilterControlProperty, value);



        public static DependencyProperty DoNotGenerateFilterControlProperty =
            DependencyProperty.RegisterAttached("DoNotGenerateFilterControl", typeof(bool), typeof(DataGridColumn), new PropertyMetadata(false));

        public static bool GetDoNotGenerateFilterControl(DependencyObject target)
            => (bool)target.GetValue(DoNotGenerateFilterControlProperty);

        public static void SetDoNotGenerateFilterControl(DependencyObject target, bool value)
            => target.SetValue(DoNotGenerateFilterControlProperty, value);
    }
}
