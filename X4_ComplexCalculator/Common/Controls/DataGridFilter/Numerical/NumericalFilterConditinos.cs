namespace X4_ComplexCalculator.Common.Controls.DataGridFilter.Numerical
{
    /// <summary>
    /// 数値用フィルタの一致条件
    /// </summary>
    public enum NumericalFilterConditinos
    {
        Equals,                 // 指定の値に等しい
        NotEquals,              // 指定の値に等しくない
        GreaterThan,            // 指定の値を超える
        GreaterThanOrEqualTo,   // 指定の値以上
        LessThan,               // 指定の値未満
        LessThanOrEqualTo,      // 指定の値以下
        Between,                // 指定の範囲内
    }
}
