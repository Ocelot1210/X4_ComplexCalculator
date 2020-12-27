namespace X4_ComplexCalculator.Common.Controlls.DataGridFilter.List
{
    /// <summary>
    /// 文字列用フィルタの一致条件
    /// </summary>
    enum StringFilterConditions
    {
        Contains,           // 文字列を含む
        NotContains,        // 文字列を含まない
        Equals,             // 文字列と一致する
        NotEquals,          // 文字列と一致しない
        StartWith,          // 文字列で始まる
        EndWith,            // 文字列で終わる
    }
}
