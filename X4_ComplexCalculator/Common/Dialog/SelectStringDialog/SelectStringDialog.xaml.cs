using System;
using System.Windows;
using System.Linq;

namespace X4_ComplexCalculator.Common.Dialog.SelectStringDialog
{
    /// <summary>
    /// EditStringDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectStringDialog : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="title">ウィンドウのタイトル</param>
        /// <param name="description">テキストボックスの説明文</param>
        /// <param name="initialString">初期文字列</param>
        /// <param name="isValidInput">文字列が有効か判定する関数</param>
        private SelectStringDialog(string title, string description, string initialString, Predicate<string> isValidInput)
        {
            InitializeComponent();

            Title = title;
            DescriptionLabel.Text = description;
            DataContext = new SelectStringDialogModel(initialString, isValidInput);
        }


        /// <summary>
        /// 文字列選択画面を表示
        /// </summary>
        /// <param name="title">ウィンドウのタイトル</param>
        /// <param name="description">テキストボックスの説明文</param>
        /// <param name="initialString">初期文字列</param>
        /// <param name="isValidInput">文字列が有効か判定する関数</param>
        /// <param name="hideCancelButton">Cancelボタンを非表示にするか</param>
        /// <returns>(OKを押されたか, 選択結果)</returns>
        public static (bool, string) ShowDialog(string title, string description, string initialString = "", Predicate<string> isValidInput = null, bool hideCancelButton = false)
        {
            var wnd = new SelectStringDialog(title, description, initialString, isValidInput);

            wnd.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive) ?? Application.Current.MainWindow;

            if (hideCancelButton)
            {
                wnd.CancelButton.Visibility = Visibility.Collapsed;
            }

            var onOk = wnd.ShowDialog() == true;

            return (onOk, wnd.SelectTextBox.Text);
        }
    }
}
