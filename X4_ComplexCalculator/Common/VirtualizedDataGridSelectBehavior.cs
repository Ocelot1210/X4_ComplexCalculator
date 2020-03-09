using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Input;
using System;
using System.Collections;

namespace X4_ComplexCalculator.Common
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
                dg.SelectionChanged += SelectedItemsChanged;
            }
            else
            {
                dg.SelectionChanged -= SelectedItemsChanged;
            }
        }

        /// <summary>
        /// 選択状態変更時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SelectedItemsChanged(object sender, SelectionChangedEventArgs e)
        {
            static void setvalue(IList items, string memberName, object value)
            {
                foreach (var itm in items)
                {
                    itm.GetType().GetProperty(memberName)?.SetValue(itm, value);
                }
            }

            var memberName = GetMemberName((DependencyObject)sender);

            setvalue(e.AddedItems, memberName, true);
            setvalue(e.RemovedItems, memberName, false);
        }
    }
}
