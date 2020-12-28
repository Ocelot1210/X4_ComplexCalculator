using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions;


namespace X4_ComplexCalculator.Common.Controls.DataGridFilter.List
{
    /// <summary>
    /// ListFilter.xaml の相互作用ロジック
    /// </summary>
    public partial class ListFilter
    {
        /// <summary>
        /// 未チェック項目一覧(前回値復元用)
        /// </summary>
        private readonly HashSet<string> _UnCheckedSet = new();

        /// <summary>
        /// ListBoxの項目一覧用ビュー
        /// </summary>
        private ICollectionView? _ListBoxItemsView;


        public ListFilter()
        {
            InitializeComponent();
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }


        #region フィルタ内容
        /// <summary>
        /// フィルタ内容
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
                nameof(Filter),
                typeof(IContentFilter),
                typeof(ListFilter),
                new FrameworkPropertyMetadata(new ListContentFilter(Enumerable.Empty<string>()), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );

        public IContentFilter Filter
        {
            get => (IContentFilter)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }
        #endregion



        #region メニューが開いているか
        /// <summary>
        /// フィルタ内容
        /// </summary>
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(ListFilter),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
                )
            );

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }
        #endregion



        #region フィルタが有効か
        /// <summary>
        /// 一致条件
        /// </summary>
        public static readonly DependencyProperty IsFilterEnabledProperty =
            DependencyProperty.Register(
                nameof(IsFilterEnabled),
                typeof(bool),
                typeof(ListFilter),
                new FrameworkPropertyMetadata(false)
            );

        public bool IsFilterEnabled
        {
            get => (bool)GetValue(IsFilterEnabledProperty);
            private set => SetValue(IsFilterEnabledProperty, value);
        }
        #endregion



        #region リストボックス検索文字列
        /// <summary>
        /// フィルタ文字列
        /// </summary>
        public static readonly DependencyProperty ListBoxSearchTextProperty =
            DependencyProperty.Register(
                nameof(ListBoxSearchText),
                typeof(string),
                typeof(ListFilter),
                new FrameworkPropertyMetadata(
                    "",
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (sender, e) => ((ListFilter)sender).CheckListBoxSearchText_Changed()
                )
            );
        public string ListBoxSearchText
        {
            get => (string)GetValue(ListBoxSearchTextProperty);
            set => SetValue(ListBoxSearchTextProperty, value);
        }
        #endregion



        #region リストボックス内容
        /// <summary>
        /// リストボックス内容
        /// </summary>
        private static readonly DependencyProperty ListBoxItemsProperty =
            DependencyProperty.Register(
                nameof(ListBoxItems),
                typeof(ObservableRangeCollection<ListBoxItem>),
                typeof(ListFilter),
                new FrameworkPropertyMetadata(new ObservableRangeCollection<ListBoxItem>())
            );

        private ObservableRangeCollection<ListBoxItem> ListBoxItems
        {
            get => (ObservableRangeCollection<ListBoxItem>)GetValue(ListBoxItemsProperty);
            set => SetValue(ListBoxItemsProperty, value);
        }
        #endregion



        /// <summary>
        /// フィルタボタンクリック時のイベント
        /// </summary>
        private void PART_FilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Filterを設定する前に未チェックの項目を保存しないとチェック状態が意図しない状態になる
            _UnCheckedSet.Clear();
            foreach (var item in ListBoxItems.Where(x => !x.IsChecked))
            {
                _UnCheckedSet.Add(item.Text);
            }


            // _ListBoxItemsView を設定しないと何故かListBoxItemFilter() で ListBoxSearchText が空文字列になる
            // → コンストラクタや OnApplyTemplate() で設定すると上手く動かない
            //    TODO:原因が分かったら修正する
            _ListBoxItemsView = CollectionViewSource.GetDefaultView(ListBoxItems);
            _ListBoxItemsView.Filter = ListBoxItemFilter;

            var srcValues = ((DataGridFilterColumnControl)TemplatedParent).SourceValues;
            ListBoxItems.Reset(srcValues.Distinct().OrderBy(x => x).Select(x => new ListBoxItem(x, !_UnCheckedSet.Contains(x))));
            IsOpen = true;
        }


        /// <summary>
        /// OKボタンクリック時のイベント
        /// </summary>
        private void PART_OKButton_Click(object sender, RoutedEventArgs e)
        {
            var filter = new ListContentFilter(ListBoxItems.Where(x => !x.IsChecked).Select(x => x.Text));
            Filter = filter;
            IsFilterEnabled = filter.IsEnabled;
            IsOpen = false;
        }


        /// <summary>
        /// キャンセルボタンクリック時のイベント
        /// </summary>
        private void PART_CancelButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ListBoxItems)
            {
                item.IsChecked = !_UnCheckedSet.Contains(item.Text);
            }
            IsOpen = false;
        }


        /// <summary>
        /// クリアボタンクリック時のイベント
        /// </summary>
        private void PART_ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _UnCheckedSet.Clear();
            foreach (var item in ListBoxItems)
            {
                item.IsChecked = true;
            }
            PART_OKButton_Click(sender, e);
        }



        /// <summary>
        /// CheckListBox検索文字列変更時
        /// </summary>
        private void CheckListBoxSearchText_Changed()
        {
            _ListBoxItemsView?.Refresh();
        }


        /// <summary>
        /// フィルタイベント
        /// </summary>
        private bool ListBoxItemFilter(object obj)
        {
            var ret = false;

            if (obj is ListBoxItem item)
            {
                ret = ListBoxSearchText == "" | 0 <= item.Text.IndexOf(ListBoxSearchText, StringComparison.InvariantCultureIgnoreCase);
                item.IsChecked = ret && !_UnCheckedSet.Contains(item.Text);
            }

            return ret;
        }
    }
}
