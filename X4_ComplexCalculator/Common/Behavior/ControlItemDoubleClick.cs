using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace X4_ComplexCalculator.Common.Behavior;

/// <summary>
/// ItemsControlにダブルクリックを処理させるかを設定する添付プロパティ
/// </summary>
public class ControlItemDoubleClick : DependencyObject
{
    /// <summary>
    /// ItemsControlにダブルクリックを処理させるか
    /// </summary>
    public static readonly DependencyProperty ItemsDoubleClickProperty =
        DependencyProperty.RegisterAttached("ItemsDoubleClick", typeof(bool), typeof(Binding));


    /// <summary>
    /// ItemsControlにダブルクリックを処理させるかを設定
    /// </summary>
    /// <param name="element"></param>
    /// <param name="value"></param>
    public static void SetItemsDoubleClick(ItemsControl element, bool value)
    {
        element.SetValue(ItemsDoubleClickProperty, value);

        if (value)
        {
            element.PreviewMouseDoubleClick += Element_PreviewMouseDoubleClick;
        }
        else
        {
            element.PreviewMouseDoubleClick -= Element_PreviewMouseDoubleClick;
        }
    }


    /// <summary>
    /// ItemsControlにダブルクリックを処理させるかを取得
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static bool GetItemsDoubleClick(ItemsControl element)
    {
        return (bool)element.GetValue(ItemsDoubleClickProperty);
    }



    /// <summary>
    /// マウスダブルクリック時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void Element_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ItemsControl control)
        {
            return;
        }

        var mouseBindings = control.InputBindings.OfType<MouseBinding>()
                                                 .Where(x => x.Gesture is not null &&
                                                             ((MouseGesture)x.Gesture).MouseAction == MouseAction.LeftDoubleClick &&
                                                             x.Command.CanExecute(null));

        foreach (var b in mouseBindings)
        {
            b.Command.Execute(null);
            e.Handled = true;
        }
    }
}
