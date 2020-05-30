using System.Windows;


namespace X4_ComplexCalculator.Common.Localize
{
    public class Localize
    {
        /// <summary>
        /// ローカライズされたメッセージボックスを表示
        /// </summary>
        /// <param name="messageBoxTextKey">表示文字列用キー</param>
        /// <param name="captionKey">タイトル部分用キー</param>
        /// <param name="button">ボタンのスタイル</param>
        /// <param name="icon">アイコンのスタイル</param>
        /// <param name="defaultResult">フォーカスするボタン</param>
        /// <param name="param">表示文字列用パラメータ</param>
        /// <returns>MessageBox.Showの戻り値</returns>
        public static MessageBoxResult ShowMessageBox(
            string              messageBoxTextKey, 
            string              captionKey      = "",
            MessageBoxButton    button          = MessageBoxButton.OK,
            MessageBoxImage     icon            = MessageBoxImage.None,
            MessageBoxResult    defaultResult   = MessageBoxResult.OK,
            params object[] param)
        {
            var format = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject(messageBoxTextKey, null, null);
            
            var caption = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject(captionKey, null, null);

            return MessageBox.Show(string.Format(format, param), caption, button, icon, defaultResult);
        }
    }
}
