using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace X4_ComplexCalculator.Common.Behaviors;


/// <summary>
/// セルにフォーカスを当てる添付ビヘイビア
/// </summary>
/// <remarks>
/// 参考URL：https://blog.magnusmontin.net/2013/11/08/how-to-programmatically-select-and-focus-a-row-or-cell-in-a-datagrid-in-wpf/
/// </remarks>
public sealed partial class DataGridFocusCellBehavior : ObservableObject
{
    #region セルフォーカス用プロパティ
    /// <summary>
    /// セルフォーカス用コマンドが設定済みか
    /// </summary>
    public static readonly DependencyProperty HasFocusCommandProperty =
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
        if (sender is not DataGrid dataGrid)
        {
            throw new ArgumentException("DependencyObject is not DataGrid", nameof(sender));
        }

        // コマンド未設定か？
        var hasCommand = (bool)sender.GetValue(HasFocusCommandProperty);
        if (hasCommand == false)
        {
            var tmp = new DataGridFocusCellBehavior(dataGrid);

            // 未設定の場合、コマンドを設定
            sender.SetValue(HasFocusCommandProperty, true);
            sender.SetValue(FocusCommandProperty, tmp.FocusCommand);
        }

        return GetFocusCommand(sender);
    }


    /// <summary>
    /// フォーカス対象の <see cref="DataGrid"/>
    /// </summary>
    private readonly DataGrid _dataGrid;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="dataGrid">フォーカス対象の <see cref="DataGrid"/></param>
    private DataGridFocusCellBehavior(DataGrid dataGrid)
    {
        _dataGrid = dataGrid;
    }


    /// <summary>
    /// セルフォーカスメイン処理
    /// </summary>
    /// <param name="param">DataGrid, 行番号, 列番号のタプル</param>
    [RelayCommand]
    private void Focus(object param)
    {
        if (param is int rowIdx)
        {
            // 未選択なら何もしない
            if (_dataGrid.CurrentCell.Column is null) return;

            var clmIdx = _dataGrid.CurrentCell.Column.DisplayIndex;

            // 行列が範囲外の場合、何もしない
            if (rowIdx < 0 || clmIdx < 0) return;


            // UpdateLayout()とScrollIntoView()しないとrowが取れない
            _dataGrid.UpdateLayout();
            _dataGrid.ScrollIntoView(_dataGrid.Items[rowIdx]);

            if (_dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIdx) is DataGridRow row)
            {
                // フォーカス対象のセルを取得してフォーカス
                GetCell(row, clmIdx)?.Focus();
                row.Focus();
            }
        }
    }


    /// <summary>
    /// 指定した行、列のセルを取得
    /// </summary>
    /// <param name="row">行</param>
    /// <param name="column">列番号</param>
    /// <returns>セル</returns>
    private DataGridCell? GetCell(DataGridRow row, int column)
    {
        if (row is not null)
        {
            var presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if (presenter is null)
            {
                row.ApplyTemplate();
                presenter = FindVisualChild<DataGridCellsPresenter>(row);
            }
            if (presenter is not null)
            {
                var cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                if (cell is not null)
                {
                    _dataGrid.ScrollIntoView(row, _dataGrid.Columns[column]);
                    cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                }
                return cell;
            }
        }

        return null;
    }


    /// <summary>
    /// VisualTreeの子要素を取得
    /// </summary>
    /// <typeparam name="T">子要素の型</typeparam>
    /// <param name="obj">子要素検索対象</param>
    /// <returns>子要素(なければnull)</returns>
    private static T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
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
                if (childOfChild is not null)
                {
                    return childOfChild;
                }
            }
        }
        return null;
    }
}
