using System;
using System.Windows;
using X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions;

namespace X4_ComplexCalculator.Common.Controlls.DataGridFilter.Numerical
{
    /// <summary>
    /// NumericalFilter.xaml の相互作用ロジック
    /// </summary>
    public partial class NumericalFilter
    {
        /// <summary>
        /// 画面前回値
        /// </summary>
        private (string, string, NumericalFilterConditinos) _PrevValue;

        public NumericalFilter()
        {
            InitializeComponent();
        }


        #region フィルタ内容
        /// <summary>
        /// フィルタ内容
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
                nameof(Filter),
                typeof(IContentFilter),
                typeof(NumericalFilter),
                new FrameworkPropertyMetadata(new NumericalContentFilter(null, NumericalFilterConditinos.Equals), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
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
                new FrameworkPropertyMetadata(NumericalFilterConditinos.Equals, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
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



        /// <summary>
        /// OKボタンクリック時のイベント
        /// </summary>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            var canClose = true;

            if (Conditions != NumericalFilterConditinos.Between)
            {
                var (value, isValidInput) = ParseDouble(FilterText1);

                if (isValidInput)
                {
                    Filter = new NumericalContentFilter(value, Conditions);
                }

                IsFilterEnabled = FilterText1 != "";
            }
            else
            {
                IsFilterEnabled = FilterText1 != "" || FilterText2 != "";
            }


            IsOpen = !canClose;
        }


        /// <summary>
        /// 文字列をdouble型に変換する
        /// </summary>
        /// <param name="text">変換対象</param>
        /// <returns>変換結果と有効な入力だったかのタプル</returns>
        private static (decimal?, bool) ParseDouble(string text)
        {
            if (decimal.TryParse(text, out var result))
            {
                return (result, true);
            }

            // 空文字列以外でパースに失敗するのはNG
            return (null, text == "");
        }



        /// <summary>
        /// キャンセルボタンクリック時のイベント
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            FilterText1 = _PrevValue.Item1;
            FilterText2 = _PrevValue.Item2;
            Conditions = _PrevValue.Item3;

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
