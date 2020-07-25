using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace X4_ComplexCalculator.Common.Behavior
{
    /// <summary>
    /// セルにフォーカスを当てる添付ビヘイビア
    /// </summary>
    /// <remarks>
    /// 参考URL：https://blog.magnusmontin.net/2013/11/08/how-to-programmatically-select-and-focus-a-row-or-cell-in-a-datagrid-in-wpf/
    /// </remarks>
    public class DataGridFocusCellBehavior
    {
        #region セルフォーカス用プロパティ
        /// <summary>
        /// セルフォーカス用コマンドが設定済みか
        /// </summary>
        private static readonly DependencyProperty HasFocusCommandProperty =
            DependencyProperty.RegisterAttached("HasFocusCommand", typeof(bool), typeof(DataGridFocusCellBehavior), new PropertyMetadata(false));


        /// <summary>
        /// セルフォーカス用コマンド
        /// </summary>
        public static readonly DependencyProperty FocusCommandProperty =
            DependencyProperty.RegisterAttached("FocusCommand", typeof(ICommand), typeof(DataGridFocusCellBehavior), new PropertyMetadata(null, FocusPropertyChanged, CoerceFocusCommand));


        /// <summary>
        /// セルフォーカス用コマンド取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ICommand? GetFocusCommand(DependencyObject obj) => obj.GetValue(FocusCommandProperty) as ICommand;


        /// <summary>
        /// セルフォーカス用コマンド設定
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="command"></param>
        public static void SetFocusCommand(DependencyObject obj, ICommand command) => obj.SetValue(FocusCommandProperty, command);


        /// <summary>
        /// セルフォーカス用コマンド変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void FocusPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // 何もしない
        }
        #endregion


        /// <summary>
        /// セルフォーカス用コマンド既定値
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static object? CoerceFocusCommand(DependencyObject sender, object value)
        {
            // コマンド未設定か？
            var hasCommand = (bool)sender.GetValue(HasFocusCommandProperty);
            if (hasCommand == false)
            {
                // 未設定の場合
                var command = new DelegateCommand<object>(FocusCommandExecute);

                // コマンドを設定済みにする
                sender.SetValue(HasFocusCommandProperty, true);

                // コマンドを設定
                sender.SetValue(FocusCommandProperty, command);
            }

            return GetFocusCommand(sender);
        }


        /// <summary>
        /// セルフォーカスメイン処理
        /// </summary>
        /// <param name="param">DataGrid, 行番号, 列番号のタプル</param>
        private static void FocusCommandExecute(object param)
        {
            if (param is Tuple<DataGrid, int, int> tuple)
            {
                DataGrid dataGrid;      // フォーカス対象DataGrid
                int rowIdx;             // 行番号
                int clmIdx;             // 列番号

                (dataGrid, rowIdx, clmIdx) = tuple;

                // 行列が範囲外の場合、何もしない
                if (rowIdx < 0 || clmIdx < 0)
                {
                    return;
                }

                // UpdateLayout()とScrollIntoView()しないとrowが取れない
                dataGrid.UpdateLayout();
                dataGrid.ScrollIntoView(dataGrid.Items[rowIdx]);

                if (dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIdx) is DataGridRow row)
                {
                    // フォーカス対象のセルを取得してフォーカス
                    GetCell(dataGrid, row, clmIdx)?.Focus();
                    row.Focus();
                }
            }
        }


        /// <summary>
        /// VisualTreeの子要素を取得
        /// </summary>
        /// <typeparam name="T">子要素の型</typeparam>
        /// <param name="obj">子要素検索対象</param>
        /// <returns>子要素(なければnull)</returns>
        private static T? FindVisualChild<T>(DependencyObject? obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T t)
                {
                    return t;
                }
                else
                {
                    var childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// 指定した行、列のセルを取得
        /// </summary>
        /// <param name="grid">取得対象DataGrid</param>
        /// <param name="row">行</param>
        /// <param name="column">列番号</param>
        /// <returns>セル</returns>
        private static DataGridCell? GetCell(DataGrid grid, DataGridRow row, int column)
        {
            if (row != null)
            {
                var presenter = FindVisualChild<DataGridCellsPresenter>(row);
                if (presenter == null)
                {
                    row.ApplyTemplate();
                    presenter = FindVisualChild<DataGridCellsPresenter>(row);
                }
                if (presenter != null)
                {
                    var cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                    if (cell != null)
                    {
                        grid.ScrollIntoView(row, grid.Columns[column]);
                        cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                    }
                    return cell;
                }
            }

            return null;
        }
    }
}
