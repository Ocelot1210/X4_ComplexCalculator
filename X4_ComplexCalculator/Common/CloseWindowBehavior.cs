using System.Windows;

namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// Windowクローズ用添付ビヘイビア
    /// </summary>
    public static class CloseWindowBehavior
    {
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
                wnd.Close();
            }
        }
    }
}
