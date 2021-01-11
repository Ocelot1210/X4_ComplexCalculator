using Prism.Mvvm;

namespace X4_ComplexCalculator.Common.Controls.DataGridFilter.MultiList
{
    /// <summary>
    /// リスト用のアイテム
    /// </summary>
    class ListBoxItem : BindableBase
    {
        #region メンバ
        /// <summary>
        /// チェックされたか
        /// </summary>
        private bool _IsChecked;
        #endregion


        #region プロパティ
        /// <summary>
        /// 表示文字列
        /// </summary>
        public string Text { get; }


        /// <summary>
        /// チェックされたか
        /// </summary>
        public bool IsChecked
        {
            get => _IsChecked;
            set => SetProperty(ref _IsChecked, value);
        }
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="text">表示文字列</param>
        /// <param name="isChecked">チェックされたか</param>
        public ListBoxItem(string text, bool isChecked = true)
        {
            Text = text;
            IsChecked = isChecked;
        }
    }
}
