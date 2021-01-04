using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace X4_ComplexCalculator.Common.Behavior
{
    /// <summary>
    /// 参考：https://dotnetmemo.hatenadiary.org/entry/20120212/1329061121
    /// </summary>
    public class DataGridHeaderThumbBehavior
    {
        public static readonly DependencyProperty SyncColumnProperty =
            DependencyProperty.RegisterAttached("SyncColumn", typeof(DataGridColumn), typeof(DataGridHeaderThumbBehavior), new PropertyMetadata(PropertyCallback));

        public static DataGridColumn GetSyncColumn(Thumb obj)
            => (DataGridColumn)obj.GetValue(SyncColumnProperty);

        public static void SetSyncColumn(Thumb obj, DataGridColumn value)
            => obj.SetValue(SyncColumnProperty, value);


        private static void PropertyCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is not Thumb source)
            {
                return;
            }

            void eventHandler(object sender, DragDeltaEventArgs e)
            {
                var target = GetSyncColumn(source);
                target.Width = new DataGridLength(target.ActualWidth + e.HorizontalChange);
            }

            if (args.OldValue is not null)
            {
                source.DragDelta -= eventHandler;
            }

            if (args.NewValue is not null)
            {
                source.DragDelta += eventHandler;
            }
        }


    }
}
