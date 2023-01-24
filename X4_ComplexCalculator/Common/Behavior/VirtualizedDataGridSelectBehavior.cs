using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace X4_ComplexCalculator.Common.Behavior;

/// <summary>
/// 仮想化を行ったDataGridも正しく選択できるようにするビヘイビア
/// </summary>
public class VirtualizedDataGridSelectBehavior
{
    /// <summary>
    /// 添付ビヘイビアの有効/無効を設定するメンバ名
    /// </summary>
    public static DependencyProperty EnabledProperty =
        DependencyProperty.RegisterAttached("VirtualizedSelectionEnabled", typeof(bool), typeof(VirtualizedDataGridSelectBehavior), new PropertyMetadata(VirtualizedSelectionEnabledProppertyChanged));


    /// <summary>
    /// 添付ビヘイビアの有効/無効状態を取得する
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public static void SetVirtualizedSelectionEnabled(DependencyObject obj, bool value) => obj.SetValue(EnabledProperty, value);


    /// <summary>
    /// 添付ビヘイビアの有効/無効状態を設定する
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool GetVirtualizedSelectionEnabled(DependencyObject obj) => (bool)obj.GetValue(EnabledProperty);



    /// <summary>
    /// 選択状態を設定するメンバ名変更時
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="e"></param>
    private static void VirtualizedSelectionEnabledProppertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not DataGrid dg)
        {
            return;
        }

        // 添付ビヘイビア有効化？
        if (GetVirtualizedSelectionEnabled(obj))
        {
            // 有効化の場合、選択状態変更時のイベントハンドラの登録
            dg.SelectionChanged += SelectedItemsChanged;
            dg.SelectedCellsChanged += SelectedCellsChanged;
        }
        else
        {
            // 無効化の場合、選択状態変更時のイベントハンドラの登録解除
            dg.SelectionChanged -= SelectedItemsChanged;
            dg.SelectedCellsChanged -= SelectedCellsChanged;
        }
    }


    /// <summary>
    /// 選択セル変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
    {
        if (sender is not DataGrid dataGrid)
        {
            return;
        }

        // セル選択モード以外の場合
        if (dataGrid.SelectionUnit != DataGridSelectionUnit.Cell)
        {
            // 選択解除されたセルがある場合、ここで選択状態を解除する
            // → 解除しないとdelキーを押した時に意図しない要素が削除される
            if (0 < e.RemovedCells.Count)
            {
                SetSelectedStatus(e.RemovedCells.Select(x => x.Item), false);
            }
            return;
        }

        using (Dispatcher.CurrentDispatcher.DisableProcessing())
        {
            SetSelectedStatus(e.AddedCells.Select(x => x.Item), true);

            // 1行分解除された項目がある場合
            var allRemovedItems = GetUnselectedItems(dataGrid.Columns.Count - 1, e.RemovedCells);
            if (allRemovedItems.Any())
            {
                SetSelectedStatus(allRemovedItems, false);
            }
            else
            {
                SetSelectedStatus(e.RemovedCells.Select(x => x.Item), false);
            }
        }
    }


    /// <summary>
    /// 1行分選択解除された項目を取得
    /// </summary>
    /// <param name="columns">列数</param>
    /// <param name="cell">削除候補セル一覧</param>
    /// <returns>選択解除要素一覧</returns>
    private static IEnumerable<object> GetUnselectedItems(int columns, IEnumerable<DataGridCellInfo> cell)
    {
        object? currentItem = null;         // セルの親オブジェクト
        var clmCnt = 0;                     // 列数カウンタ

        foreach (var item in cell.Select(x => x.Item))
        {
            // セルの親オブジェクト(DataGrid1行分のオブジェクト)が変わったらカウンタをリセットする
            if (item != currentItem)
            {
                currentItem = item;
                clmCnt = 0;
            }

            // 選択削除されたセル数 == 列数の場合、その行の親オブジェクトを返す
            // → 行に対応するセルが全て選択解除されたためその行のオブジェクトを選択解除する
            if (clmCnt == columns)
            {
                yield return item;
            }
            clmCnt++;
        }
    }


    /// <summary>
    /// 選択状態変更時のイベント
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void SelectedItemsChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not DataGrid dataGrid)
        {
            return;
        }

        // 行選択モードでなければ何もしない
        if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
        {
            return;
        }

        using (Dispatcher.CurrentDispatcher.DisableProcessing())
        {
            SetSelectedStatus(e.AddedItems, true);
            SetSelectedStatus(e.RemovedItems, false);
        }
    }


    /// <summary>
    /// 選択状態を設定
    /// </summary>
    /// <param name="items">設定対象</param>
    /// <param name="isSelected">設定値(選択されたか)</param>
    private static void SetSelectedStatus(IEnumerable items, bool isSelected)
    {
        foreach (var item in items.OfType<ISelectable>())
        {
            item.IsSelected = isSelected;
        }
    }
}
