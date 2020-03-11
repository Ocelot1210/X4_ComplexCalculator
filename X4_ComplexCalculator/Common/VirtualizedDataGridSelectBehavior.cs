using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

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
            static void setvalue(IList items, MethodInfo method, object value)
            {
                var setValue = new object[] { value };

                foreach (var itm in items)
                {
                    method.Invoke(itm, setValue);
                }
            }

            // 高速化のためここでプロパティのdelegateを取得して使い回す
            MethodInfo methodInfo;
            {
                var memberName = GetMemberName((DependencyObject)sender);

                var obj = (0 < e.AddedItems.Count) ? e.AddedItems[0] : e.RemovedItems[0];

                methodInfo = obj?.GetType()
                                ?.GetProperty(memberName)
                                ?.GetSetMethod();
            }

            if (methodInfo == null)
            {
                return;
            }

            setvalue(e.AddedItems, methodInfo, true);
            setvalue(e.RemovedItems, methodInfo, false);
        }
    }
}
