using System.Windows;

namespace X4_ComplexCalculator.Common;

/// <summary>
/// データ中継用クラス
/// </summary>
public class BindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }


    /// <summary>
    /// 中継するデータ
    /// </summary>
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));


    /// <summary>
    /// 中継するデータ
    /// </summary>
    public object Data
    {
        get => GetValue(DataProperty) ?? DefaultData;
        set => SetValue(DataProperty, value);
    }


    /// <summary>
    /// 中継するデータ(初期値)
    /// </summary>
    public static readonly DependencyProperty DefaultDataProperty =
        DependencyProperty.Register("DefaultData", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));


    /// <summary>
    /// 中継するデータ
    /// </summary>
    public object DefaultData
    {
        get => GetValue(DefaultDataProperty);
        set => SetValue(DefaultDataProperty, value);
    }
}
