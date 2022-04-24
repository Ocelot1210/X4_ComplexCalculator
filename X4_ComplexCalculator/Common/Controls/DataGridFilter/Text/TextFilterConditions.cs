namespace X4_ComplexCalculator.Common.Controls.DataGridFilter.Text;

/// <summary>
/// 文字列用フィルタの一致条件
/// </summary>
public enum TextFilterConditions
{
    Contains,           // 文字列を含む
    NotContains,        // 文字列を含まない
    Equals,             // 文字列と一致する
    NotEquals,          // 文字列と一致しない
    StartWith,          // 文字列で始まる
    EndWith,            // 文字列で終わる
}
