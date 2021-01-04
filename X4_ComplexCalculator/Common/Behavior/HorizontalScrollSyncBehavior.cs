using System.Windows;
using System.Windows.Controls;

namespace X4_ComplexCalculator.Common.Behavior
{
    public class HorizontalScrollSyncBehavior
    {
        public static readonly DependencyProperty SyncElementProperty =
            DependencyProperty.RegisterAttached("SyncElement", typeof(ScrollViewer), typeof(HorizontalScrollSyncBehavior), new PropertyMetadata(PropertyCallback));

        public static ScrollViewer GetSyncElement(ScrollViewer obj)
            => (ScrollViewer)obj.GetValue(SyncElementProperty);


        public static void SetSyncElement(ScrollViewer obj, ScrollViewer value)
            => obj.SetValue(SyncElementProperty, value);


        private static void PropertyCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is not ScrollViewer source)
            {
                return;
            }

            void eventHandler(object sender, ScrollChangedEventArgs e)
            {
                var target = GetSyncElement(source);
                target?.ScrollToHorizontalOffset(source.HorizontalOffset);
            }

            if (args.OldValue is ScrollViewer oldScroll)
            {
                source.ScrollChanged -= eventHandler;
            }

            if (args.NewValue is ScrollViewer newScroll)
            {
                source.ScrollChanged += eventHandler;
            }
        }
    }
}
