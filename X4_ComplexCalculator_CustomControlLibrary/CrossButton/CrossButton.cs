using System.Windows;
using System.Windows.Controls;

namespace X4_ComplexCalculator_CustomControlLibrary;

/// <summary>
/// ×マークボタン
/// </summary>
public class CrossButton : Button
{
    static CrossButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CrossButton), new FrameworkPropertyMetadata(typeof(CrossButton)));
    }
}
