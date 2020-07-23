using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using X4_ComplexCalculator.Common.Reflection;

namespace X4_ComplexCalculator.Common.Behavior
{
    /// <summary>
    /// 仮想化を行ったDataGridも正しく選択できるようにするビヘイビア
    /// </summary>
    public class VirtualizedDataGridSelectBehavior
    {
        /// <summary>
        /// 選択状態を設定するメンバ名
        /// </summary>
        public static DependencyProperty MemberNameProperty =
            DependencyProperty.RegisterAttached("MemberName", typeof(string), typeof(VirtualizedDataGridSelectBehavior), new PropertyMetadata(MemberNameProppertyChanged));

        /// <summary>
        /// 選択状態を設定するメンバ名を設定
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetMemberName(DependencyObject obj, string value) => obj.SetValue(MemberNameProperty, value);


        /// <summary>
        /// 選択状態を設定するメンバ名を取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetMemberName(DependencyObject obj) => (string)obj.GetValue(MemberNameProperty);


        /// <summary>
        /// 選択状態を設定するメンバ名変更時
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private static void MemberNameProppertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is DataGrid dg))
            {
                return;
            }

            // 選択状態変更時のイベントハンドラの登録/解除
            if (e.NewValue != null)
            {
                dg.SelectionChanged += SelectedItemsChanged;
                dg.SelectedCellsChanged += SelectedCellsChanged;
            }
            else
            {
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
            if (!(sender is DataGrid dataGrid))
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
                    var items = e.RemovedCells.Select(x => x.Item).Distinct();
                    var accessor = items.First().GetType().GetProperty(GetMemberName((DependencyObject)sender))?.ToAccessor();

                    if (accessor != null)
                    {
                        SetSelectedStatus(items, accessor, false);
                    }
                }
                return;
            }

            using (Dispatcher.CurrentDispatcher.DisableProcessing())
            {
                var pinfo = ((0 < e.AddedCells.Count) ? e.AddedCells[0].Item : e.RemovedCells[0].Item).GetType().GetProperty(GetMemberName((DependencyObject)sender));
                var accessor = pinfo?.ToAccessor();

                if (accessor != null)
                {
                    SetSelectedStatus(e.AddedCells.Select(x => x.Item), accessor, true);
                    SetSelectedStatus(e.RemovedCells.Select(x => x.Item), accessor, false);
                }
            }
        }


        /// <summary>
        /// 選択状態変更時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SelectedItemsChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is DataGrid dataGrid))
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
                var itm = ((0 < e.AddedItems.Count) ? e.AddedItems[0] : e.RemovedItems[0]);
                if (itm != null)
                {
                    var accessor = itm.GetType().GetProperty(GetMemberName((DependencyObject)sender))?.ToAccessor();
                    if (accessor != null)
                    {
                        SetSelectedStatus(e.AddedItems, accessor, true);
                        SetSelectedStatus(e.RemovedItems, accessor, false);
                    }
                }
            }
        }


        /// <summary>
        /// 選択状態を設定
        /// </summary>
        /// <param name="items">設定対象</param>
        /// <param name="accessor">プロパティへのアクセサー</param>
        /// <param name="value">設定値</param>
        static void SetSelectedStatus(IEnumerable items, IAccessor accessor, object value)
        {
            foreach (var item in items)
            {
                if (item != null)
                {
                    accessor.SetValue(item, value);
                }
            }
        }
    }
}
