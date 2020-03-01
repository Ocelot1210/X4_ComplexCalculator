using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;

namespace CustomControlLibrary.DataGridInline
{
    /// <summary>
    /// DataGridView内に表示するためのListView
    /// スクロールイベントを親へバブリングする他、カラムヘッダのクリックでソートを行う
    /// </summary>
    public class InlineListView : ListView
    {
        #region メンバ
        /// <summary>
        /// スクロールイベントバブリング用
        /// </summary>
        ScrollViewer scrollViewer;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        static InlineListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InlineListView), new FrameworkPropertyMetadata(typeof(InlineListView)));
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // カラムヘッダクリック時のイベントを追加
            AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));

            PreviewMouseWheel += PreviewMouseWheelEventHandler;
        }




        #region カラムヘッダクリック時のソート処理関連
        /// <summary>
        /// カラムヘッダクリック時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is System.Windows.Controls.GridViewColumnHeader headerClicked))
            {
                return;
            }

            if (!(headerClicked.Column is InlineGridViewColumn column))
            {
                return;
            }


            if (string.IsNullOrEmpty(column.SortTargetPropertyName))
            {
                return;
            }

            ApplySort(Items, column.SortTargetPropertyName);
        }


        /// <summary>
        /// ソート本処理
        /// </summary>
        /// <param name="view"></param>
        /// <param name="propertyName">ソート対象のプロパティ名</param>
        public static void ApplySort(ICollectionView view, string propertyName)
        {
            // プロパティ名が空なら何もしない
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            var direction = ListSortDirection.Ascending;  // ソート方向

            // ソート方向決定
            if (view.SortDescriptions.Count > 0)
            {
                SortDescription currentSort = view.SortDescriptions[0];
                if (currentSort.PropertyName == propertyName)
                {
                    direction = (currentSort.Direction == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
                }
                view.SortDescriptions.Clear();
            }

            // ソート実行
            view.SortDescriptions.Add(new SortDescription(propertyName, direction));
        }
        #endregion


        #region マウススクロール時の処理関連
        /// <summary>
        /// マウススクロール時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviewMouseWheelEventHandler(object sender, MouseWheelEventArgs e)
        {
            // ListViewn内のScrollViewerを取得する(初回のみ)
            if (scrollViewer == null)
            {
                scrollViewer = FindVisualChild<System.Windows.Controls.ScrollViewer>(this);
            }

            var scrollPos = scrollViewer.ContentVerticalOffset;

            // スクロールすべきか？
            if ((scrollPos == scrollViewer.ScrollableHeight && e.Delta < 0) || (scrollPos == 0 && e.Delta > 0))
            {
                e.Handled = true;

                var e2 = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                e2.RoutedEvent = MouseWheelEvent;

                RaiseEvent(e2);
            }
        }

        /// <summary>
        /// VisualTreeより指定した型の子要素を取得する
        /// </summary>
        /// <typeparam name="T">取得したい子要素の型</typeparam>
        /// <param name="parent">親要素</param>
        /// <returns></returns>
        private T FindVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (var i = 0; i < childrenCount; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);

                // 見つかった要素は指定した型か？
                child = v as T;
                if (child == null)
                {
                    // 違ったらそれを親にして再度検索
                    child = FindVisualChild<T>(v);
                }

                // 欲しい物が見つかったのでbreak
                if (child != null)
                {
                    break;
                }
            }

            return child;
        }

        #endregion
    }
}
