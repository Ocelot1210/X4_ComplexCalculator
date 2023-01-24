using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace X4_ComplexCalculator.Common.Behavior;

/// <summary>
/// マウスカーソルがセルに乗った時に編集モードにするビヘイビア
/// </summary>
public class DataGridMouseEnterEditModeBehavior : Behavior<DataGridCell>
{
    /// <summary>
    /// マウスカーソル座標
    /// </summary>
    private static Point _CursorPosition;

    /// <summary>
    /// アタッチ時
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseEnter += DataGridCell_MouseEnter;
        AssociatedObject.MouseLeave += DataGridCell_MouseLeave;
        AssociatedObject.MouseMove  += DataGridCell_MouseMove;
        AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
    }


    /// <summary>
    /// セル内でマウスが動いた場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DataGridCell_MouseMove(object sender, MouseEventArgs e)
    {
        // 編集可能なセルの場合のみ処理
        if (!AssociatedObject.IsReadOnly)
        {
            _CursorPosition = Mouse.GetPosition(Application.Current.MainWindow);
        }
    }


    /// <summary>
    /// デタッチ時
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseEnter -= DataGridCell_MouseEnter;
        AssociatedObject.MouseLeave -= DataGridCell_MouseLeave;
        AssociatedObject.MouseMove  -= DataGridCell_MouseMove;
        AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
    }


    /// <summary>
    /// セルクリック時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 編集可能なセルの場合のみ処理
        if (!AssociatedObject.IsReadOnly)
        {
            AssociatedObject.IsEditing = true;
        }
    }


    /// <summary>
    /// マウスが入った場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DataGridCell_MouseEnter(object sender, MouseEventArgs e)
    {
        // 編集可能なセルの場合のみ処理
        if (!AssociatedObject.IsReadOnly)
        {
            // 前回のマウスの座標と異なれば編集モードにする(これが無いとEnterでセル移動時に意図せず編集モードになる場合がある)
            if (Mouse.GetPosition(Application.Current.MainWindow) != _CursorPosition)
            {
                AssociatedObject.IsEditing = true;
            }
        }
    }


    /// <summary>
    /// マウスが離れた場合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DataGridCell_MouseLeave(object sender, MouseEventArgs e)
    {
        if (!AssociatedObject.IsReadOnly)
        {
            AssociatedObject.IsEditing = false;
        }
    }
}
