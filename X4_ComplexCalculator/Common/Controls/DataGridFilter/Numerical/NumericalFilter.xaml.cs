using System;
using System.Windows;
using X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions;
using WPFLocalizeExtension.Engine;
using X4_ComplexCalculator.Common.Controls.DataGridFilter.Interface;

namespace X4_ComplexCalculator.Common.Controls.DataGridFilter.Numerical
{
    /// <summary>
    /// NumericalFilter.xaml の相互作用ロジック
    /// </summary>
    public partial class NumericalFilter
    {
        #region メンバ
        /// <summary>
        /// 画面前回値
        /// </summary>
        private (string, string, NumericalFilterConditinos) _PrevValue;


        /// <summary>
        /// DataContext
        /// </summary>
        private DataGridFilterColumnControl? _FilterColumnControl;
        #endregion


        public NumericalFilter()
        {
            InitializeComponent();
        }


        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _FilterColumnControl = DataContext as DataGridFilterColumnControl;

            // DataContext経由で前回のフィルタを取得
            _FilterColumnControl = DataContext as DataGridFilterColumnControl;
            if (_FilterColumnControl?.LoadFilter() is IDataGridFilter prevFilter)
            {
                Filter = prevFilter;

                // 画面の前回値復元
                if (prevFilter is NumericalContentFilter numFilter)
                {
                    FilterText1 = numFilter.InputText;
                    Conditions  = numFilter.Conditinos;
                }
                else if (prevFilter is NumericalBetweenContentFilter betweenFilter)
                {
                    FilterText1 = betweenFilter.MinValueText;
                    FilterText2 = betweenFilter.MaxValueText;
                    Conditions = NumericalFilterConditinos.Between;
                }
            }
        }


        #region フィルタ内容
        /// <summary>
        /// フィルタ内容
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
                nameof(Filter),
                typeof(IDataGridFilter),
                typeof(NumericalFilter),
                new FrameworkPropertyMetadata(new NumericalContentFilter("", NumericalFilterConditinos.Equals), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );

        public IDataGridFilter Filter
        {
            get => (IDataGridFilter)GetValue(FilterProperty);
            set
            {
                if (Filter is null || !Filter.Equals(value))
                {
                    SetValue(FilterProperty, value);
                    IsFilterEnabled = value.IsFilterEnabled;
                }

                // 仮想化対策のためフィルタを保存
                _FilterColumnControl?.SaveFilter();
            }
        }
        #endregion



        #region メニューが開いているか
        /// <summary>
        /// フィルタ内容
        /// </summary>
        private static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(NumericalFilter),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
                )
            );

        private bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }
        #endregion



        #region フィルタ文字列
        /// <summary>
        /// フィルタ文字列1 (指定値と最小値を兼用)
        /// </summary>
        private static readonly DependencyProperty FilterText1Property =
            DependencyProperty.Register(
                nameof(FilterText1),
                typeof(string),
                typeof(NumericalFilter),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );
        private string FilterText1
        {
            get => (string)GetValue(FilterText1Property);
            set => SetValue(FilterText1Property, value);
        }

        /// <summary>
        /// フィルタ文字列2(最大値用)
        /// </summary>
        private static readonly DependencyProperty FilterText2Property =
            DependencyProperty.Register(
                nameof(FilterText2),
                typeof(string),
                typeof(NumericalFilter),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );
        private string FilterText2
        {
            get => (string)GetValue(FilterText2Property);
            set => SetValue(FilterText2Property, value);
        }
        #endregion



        #region 一致条件
        /// <summary>
        /// 一致条件
        /// </summary>
        private static readonly DependencyProperty ConditionsProperty =
            DependencyProperty.Register(
                nameof(Conditions),
                typeof(NumericalFilterConditinos),
                typeof(NumericalFilter),
                new FrameworkPropertyMetadata(NumericalFilterConditinos.Equals, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (sender, e) => ((NumericalFilter)sender).Conditions_Changed())
            );

        private NumericalFilterConditinos Conditions
        {
            get => (NumericalFilterConditinos)GetValue(ConditionsProperty);
            set => SetValue(ConditionsProperty, value);
        }
        #endregion



        #region フィルタが有効か
        /// <summary>
        /// 一致条件
        /// </summary>
        private static readonly DependencyProperty IsFilterEnabledProperty =
            DependencyProperty.Register(
                nameof(IsFilterEnabled),
                typeof(bool),
                typeof(NumericalFilter),
                new FrameworkPropertyMetadata(false)
            );

        private bool IsFilterEnabled
        {
            get => (bool)GetValue(IsFilterEnabledProperty);
            set => SetValue(IsFilterEnabledProperty, value);
        }
        #endregion



        #region フィルタ文字列1のウォーターマーク
        /// <summary>
        /// フィルタ文字列1のウォーターマーク
        /// </summary>
        private static readonly DependencyProperty FilterText1_WaterMarkProperty =
            DependencyProperty.Register(
                nameof(FilterText1_WaterMark),
                typeof(string),
                typeof(NumericalFilter),
                new FrameworkPropertyMetadata((string)LocalizeDictionary.Instance.GetLocalizedObject("Lang:NumericalFilter_EnterAValue", null, null), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
            );
        private string FilterText1_WaterMark
        {
            get => (string)GetValue(FilterText1_WaterMarkProperty);
            set => SetValue(FilterText1_WaterMarkProperty, value);
        }
        #endregion



        /// <summary>
        /// 一致条件変更時のイベント
        /// </summary>
        private void Conditions_Changed()
        {
            var langID = Conditions == NumericalFilterConditinos.Between ? "Lang:NumericalFilter_EnterAMinValue" : "Lang:NumericalFilter_EnterAValue";

            FilterText1_WaterMark = (string)LocalizeDictionary.Instance.GetLocalizedObject(langID, null, null);
        }



        /// <summary>
        /// OKボタンクリック時のイベント
        /// </summary>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (Conditions != NumericalFilterConditinos.Between)
            {
                Filter = new NumericalContentFilter(FilterText1, Conditions);
            }
            else
            {
                Filter = new NumericalBetweenContentFilter(FilterText1, FilterText2);
            }

            IsOpen = false;
        }


        /// <summary>
        /// キャンセルボタンクリック時のイベント
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            FilterText1 = _PrevValue.Item1;
            FilterText2 = _PrevValue.Item2;
            Conditions  = _PrevValue.Item3;

            IsOpen = false;
        }


        /// <summary>
        /// フィルタボタンクリック時のイベント
        /// </summary>
        private void PART_FilterButton_Click(object sender, RoutedEventArgs e)
        {
            _PrevValue.Item1 = FilterText1;
            _PrevValue.Item2 = FilterText2;
            _PrevValue.Item3 = Conditions;
            IsOpen = true;
        }


        /// <summary>
        /// フィルタクリアボタンクリック時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_ClearButton_Click(object sender, RoutedEventArgs e)
        {
            FilterText1 = "";
            FilterText2 = "";
            OKButton_Click(sender, e);
        }
    }
}
