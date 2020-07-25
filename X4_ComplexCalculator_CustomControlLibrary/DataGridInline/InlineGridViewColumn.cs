using System.Windows;
using System.Windows.Controls;

namespace CustomControlLibrary.DataGridInline
{
    /// <summary>
    /// DataGridView内に表示するためのListViewのカラム用
    /// SortTargetPropertyNameを設定するとそのプロパティをベースにソートを行う
    /// </summary>
    public class InlineGridViewColumn : GridViewColumn
    {
        #region ソート対象プロパティ名用
        /// <summary>
        /// ソート対象プロパティ名
        /// </summary>
        public static readonly DependencyProperty SortTargetPropertyNameProperty =
            DependencyProperty.RegisterAttached(nameof(SortTargetPropertyName), typeof(string), typeof(InlineGridViewColumn), new UIPropertyMetadata(null));


        /// <summary>
        /// ソート対象プロパティ名
        /// </summary>
        public string SortTargetPropertyName
        {
            get
            {
                return (string)GetValue(SortTargetPropertyNameProperty);
            }
            set
            {
                SetValue(SortTargetPropertyNameProperty, value);
            }
        }
        #endregion
    }
}
