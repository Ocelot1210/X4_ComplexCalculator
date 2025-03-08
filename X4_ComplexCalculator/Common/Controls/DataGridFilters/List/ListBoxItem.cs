using CommunityToolkit.Mvvm.ComponentModel;

namespace X4_ComplexCalculator.Common.Controls.DataGridFilters.List;

/// <summary>
/// リスト用のアイテム
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="text">表示文字列</param>
/// <param name="isChecked">チェックされたか</param>
sealed partial class ListBoxItem(string text, bool isChecked = true) : ObservableObject
{
    #region プロパティ
    /// <summary>
    /// 表示文字列
    /// </summary>
    public string Text { get; } = text;


    /// <summary>
    /// チェックされたか
    /// </summary>
    [ObservableProperty]
    public partial bool IsChecked { get; set; } = isChecked;
    #endregion
}
