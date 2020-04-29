using System.Windows;

namespace X4_ComplexCalculator.Common.Behavior
{
    /// <summary>
    /// Windowクローズ用添付ビヘイビア
    /// </summary>
    public static class CloseWindowBehavior
    {
        #region ウィンドウの表示状態
        /// <summary>
        /// ウィンドウの表示状態
        /// </summary>
        public static readonly DependencyProperty CloseProperty =
            DependencyProperty.RegisterAttached("Close", typeof(bool), typeof(CloseWindowBehavior), new PropertyMetadata(false, OnCloseChanged));

        /// <summary>
        /// ウィンドウの表示状態を取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetClose(DependencyObject obj)
        {
            return (bool)obj.GetValue(CloseProperty);
        }

        /// <summary>
        /// ウィンドウの表示状態を設定
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetClose(DependencyObject obj, bool value)
        {
            obj.SetValue(CloseProperty, value);
        }
        #endregion

        #region ダイアログの戻り値
        /// <summary>
        /// ダイアログの戻り値
        /// </summary>
        public static readonly DependencyProperty ResultProperty =
            DependencyProperty.RegisterAttached("Result", typeof(bool?), typeof(CloseWindowBehavior), new PropertyMetadata(null));

        /// <summary>
        /// ダイアログの戻り値を取得
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool? GetResult(DependencyObject obj)
        {
            return (bool?)obj.GetValue(ResultProperty);
        }

        /// <summary>
        /// ダイアログの戻り値を設定
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetResult(DependencyObject obj, bool? value)
        {
            obj.SetValue(ResultProperty, value);
        }
        #endregion

        /// <summary>
        /// ウィンドウの表示状態が変更された場合
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        public static void OnCloseChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Window wnd))
            {
                wnd = Window.GetWindow(obj);
            }

            if (GetClose(obj))
            {
                // モーダルダイアログの場合、戻り値を設定
                if (System.Windows.Interop.ComponentDispatcher.IsThreadModal)
                {
                    wnd.DialogResult = GetResult(obj);
                }
                wnd.Close();
            }
        }
    }
}
