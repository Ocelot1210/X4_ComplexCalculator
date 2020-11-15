using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace X4_ComplexCalculator_CustomControlLibrary.SearchBoxColumnHeader
{
    public class SearchBoxColumnHeader : DataGridColumnHeader
    {
        #region プロパティ
        /// <summary>
        /// 検索文字列
        /// </summary>
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(SearchBoxColumnHeader), new UIPropertyMetadata(null));
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }
        #endregion


        static SearchBoxColumnHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBoxColumnHeader), new FrameworkPropertyMetadata(typeof(SearchBoxColumnHeader)));
        }
    }
}
