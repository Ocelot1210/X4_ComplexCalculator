using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;
using System.Windows.Input;

namespace X4_ComplexCalculator.Common.Behavior
{
    /// <summary>
    /// マウスカーソルがセルに乗った時に編集モードにするビヘイビア
    /// </summary>
    public class DataGridMouseEnterEditModeBehavior : Behavior<DataGridCell>
    {
        /// <summary>
        /// アタッチ時
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += DataGridCell_MouseEnter;
            AssociatedObject.MouseLeave += DataGridCell_MouseLeave;
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
        }


        /// <summary>
        /// デタッチ時
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseEnter -= DataGridCell_MouseEnter;
            AssociatedObject.MouseLeave -= DataGridCell_MouseLeave;
            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
        }


        /// <summary>
        /// セルクリック時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
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
        private void DataGridCell_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!AssociatedObject.IsReadOnly)
            {
                AssociatedObject.IsEditing = true;
            }
        }

        /// <summary>
        /// マウスが離れた場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridCell_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!AssociatedObject.IsReadOnly)
            {
                AssociatedObject.IsEditing = false;
            }
        }
    }
}
