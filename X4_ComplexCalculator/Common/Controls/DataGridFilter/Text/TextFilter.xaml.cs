using System.Windows;
using X4_ComplexCalculator.Common.Controls.DataGridFilter.Interface;
using X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions;

namespace X4_ComplexCalculator.Common.Controls.DataGridFilter.Text;

/// <summary>
/// TextFilter.xaml の相互作用ロジック
/// </summary>
public partial class TextFilter
{
    #region メンバ
    /// <summary>
    /// 画面前回値
    /// </summary>
    private (string, TextFilterConditions) _PrevValue;


    /// <summary>
    /// DataContext
    /// </summary>
    private DataGridFilterColumnControl? _FilterColumnControl;
    #endregion


    public TextFilter()
    {
        InitializeComponent();
    }


    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _FilterColumnControl = DataContext as DataGridFilterColumnControl;

        // DataContext経由で前回のフィルタを取得
        _FilterColumnControl = DataContext as DataGridFilterColumnControl;
        var prevFilter = _FilterColumnControl?.LoadFilter();
        if (prevFilter is TextContentFilter filter)
        {
            Filter = filter;
            FilterText = filter.FilterText;
            Conditions = filter.Conditions;
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
            typeof(TextFilter),
            new FrameworkPropertyMetadata(new TextContentFilter("", TextFilterConditions.Contains), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
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
            typeof(TextFilter),
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
    /// フィルタ文字列
    /// </summary>
    private static readonly DependencyProperty FilterTextProperty =
        DependencyProperty.Register(
            nameof(FilterText),
            typeof(string),
            typeof(TextFilter),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );
    private string FilterText
    {
        get => (string)GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }
    #endregion



    #region 一致条件
    /// <summary>
    /// 一致条件
    /// </summary>
    private static readonly DependencyProperty ConditionsProperty =
        DependencyProperty.Register(
            nameof(Conditions),
            typeof(TextFilterConditions),
            typeof(TextFilter),
            new FrameworkPropertyMetadata(TextFilterConditions.Contains, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

    private TextFilterConditions Conditions
    {
        get => (TextFilterConditions)GetValue(ConditionsProperty);
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
            typeof(TextFilter),
            new FrameworkPropertyMetadata(false)
        );

    private bool IsFilterEnabled
    {
        get => (bool)GetValue(IsFilterEnabledProperty);
        set => SetValue(IsFilterEnabledProperty, value);
    }
    #endregion





    /// <summary>
    /// フィルタボタンクリック時のイベント
    /// </summary>
    private void PART_FilterButton_Click(object sender, RoutedEventArgs e)
    {
        IsOpen = true;
        _PrevValue.Item1 = FilterText;
        _PrevValue.Item2 = Conditions;
    }


    /// <summary>
    /// OKボタンクリック時のイベント
    /// </summary>
    private void PART_OKButton_Click(object sender, RoutedEventArgs e)
    {
        Filter = new TextContentFilter(FilterText, Conditions);
        IsOpen = false;
    }


    /// <summary>
    /// キャンセルボタンクリック時のイベント
    /// </summary>
    private void PART_CancelButton_Click(object sender, RoutedEventArgs e)
    {
        FilterText = _PrevValue.Item1;
        Conditions = _PrevValue.Item2;
        IsOpen = false;
    }


    /// <summary>
    /// クリアボタンクリック時のイベント
    /// </summary>
    private void PART_ClearButton_Click(object sender, RoutedEventArgs e)
    {
        FilterText = "";
        PART_OKButton_Click(sender, e);
    }
}
