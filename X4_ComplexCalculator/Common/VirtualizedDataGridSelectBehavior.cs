using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// 仮想化を行ったDataGridも正しく選択できるようにするビヘイビア
    /// </summary>
    class VirtualizedDataGridSelectBehavior
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
        /// <param name="d"></param>
        /// <param name="e"></param>
        public static void MemberNameProppertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DataGrid dg))
            {
                return;
            }

            // 選択状態変更時のイベントハンドラの登録/解除
            if (e.NewValue != null)
            {
                dg.SelectedCellsChanged += SelectedCellsChanged;
            }
            else
            {
                dg.SelectedCellsChanged -= SelectedCellsChanged;
            }
        }

        /// <summary>
        /// セル選択状態変更時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            static void setvalue(IEnumerable<object> items, string memberName, object value)
            {
                object oldItm = null;

                foreach (var itm in items)
                {
                    if (itm != oldItm)
                    {
                        itm.GetType().GetProperty(memberName)?.SetValue(itm, value);
                        oldItm = itm;
                    }
                }
            }

            var memberName = GetMemberName((DependencyObject)sender);

            setvalue(e.AddedCells.Select(x => x.Item),   memberName, true);
            setvalue(e.RemovedCells.Select(x => x.Item), memberName, false);
        }
    }
}
