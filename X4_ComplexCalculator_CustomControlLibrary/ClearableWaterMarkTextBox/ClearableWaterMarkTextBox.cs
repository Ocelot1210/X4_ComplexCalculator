using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace X4_ComplexCalculator_CustomControlLibrary;

/// <summary>
/// ×ボタンで内容を消せるWaterMarkTextBox
/// </summary>
public class ClearableWaterMarkTextBox : WatermarkTextBox
{
    #region メンバ
    /// <summary>
    /// 内容クリアボタン
    /// </summary>
    private Button? _clearButton;
    #endregion

    /// <summary>
    /// コンストラクタ
    /// </summary>
    static ClearableWaterMarkTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ClearableWaterMarkTextBox),
            new FrameworkPropertyMetadata(typeof(ClearableWaterMarkTextBox))
        );
    }


    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 前のテンプレートのコントロールの後処理
        if (_clearButton != null)
        {
            _clearButton.Click -= ClearButton_Click;
        }

        // テンプレートからコントロールの取得
        _clearButton = GetTemplateChild("PART_ClearButton") as Button;

        // イベントハンドラの登録
        if (_clearButton != null)
        {
            _clearButton.Click += ClearButton_Click;
        }
    }


    /// <summary>
    /// テキストクリアボタンクリック時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        Text = "";
        Focus();        // WaterMarkが一瞬表示される対策用
    }
}
