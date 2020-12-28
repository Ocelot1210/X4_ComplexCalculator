using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace X4_ComplexCalculator.Common.Behavior
{
    /// <summary>
    /// カレントセルを自動的に編集モードにするBehavior
    /// </summary>
    public class DataGridCurrentCellEditModeBehavior : Behavior<DataGrid>
    {
        /// <summary>
        /// カレントセルを編集モードにするかのプロパティ
        /// </summary>
        public static DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(VirtualizedDataGridSelectBehavior), new PropertyMetadata(EnabledProppertyChanged));

        /// <summary>
        /// カレントセルを編集モードにするかを設定
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetEnabled(DependencyObject obj, bool value) => obj.SetValue(EnabledProperty, value);


        /// <summary>
        /// カレントセルを編集モードにするかを取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetEnabled(DependencyObject obj) => (bool)obj.GetValue(EnabledProperty);


        /// <summary>
        /// カレントセルを編集モードにするかの変更時
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private static void EnabledProppertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is not DataGrid dg)
            {
                return;
            }

            // カレントセル変更時のイベントハンドラの登録/解除
            if (e.NewValue is not null)
            {
                dg.CurrentCellChanged += DataGrid_CurrentCellChanged;
            }
            else
            {
                dg.CurrentCellChanged -= DataGrid_CurrentCellChanged;
            }
        }


        /// <summary>
        /// カレントセル変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DataGrid_CurrentCellChanged(object? sender, EventArgs e)
        {
            if (sender is not DataGrid dg || dg.CurrentCell == null || dg.CurrentCell.Column is null)
            {
                return;
            }

            // セルが読み取り専用でなければ編集モードにする
            if (GetEnabled((DependencyObject)sender) && dg.CurrentCell.Column.GetCellContent(dg.CurrentCell.Item)?.Parent is DataGridCell cell && !cell.IsReadOnly)
            {
                cell.IsEditing = true;
            }
        }
    }
}
