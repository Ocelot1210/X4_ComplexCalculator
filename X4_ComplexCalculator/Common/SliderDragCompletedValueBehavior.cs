using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace X4_ComplexCalculator.Common
{
    public class SliderDragCompletedValueBehavior
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(long), typeof(SliderDragCompletedValueBehavior), new PropertyMetadata(ValuePropertyChanged));

        
        public static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Slider sld))
            {
                return;
            }

            if (e.NewValue != null)
            {
                sld.DragLeave += Slider_DragLeave;
            }
            else
            {
                sld.DragLeave -= Slider_DragLeave;
            }
        }

        private static void Slider_DragLeave(object sender, DragEventArgs e)
        {
            SetValue((DependencyObject)sender, (long)((Slider)sender).Value);
        }

        public static void SetValue(DependencyObject obj, long value) => obj.SetValue(ValueProperty, value);

        public static long GetValue(DependencyObject obj) => (long)obj.GetValue(ValueProperty);


    }
}
