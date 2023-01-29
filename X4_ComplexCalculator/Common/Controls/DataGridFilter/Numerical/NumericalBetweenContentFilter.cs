using System;
using X4_ComplexCalculator.Common.Controls.DataGridFilter.Interface;
using X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions;

namespace X4_ComplexCalculator.Common.Controls.DataGridFilter.Numerical;

/// <summary>
/// 数値フィルタ用クラス(指定の範囲内用)
/// </summary>
class NumericalBetweenContentFilter : IDataGridFilter
{
    #region メンバ
    /// <summary>
    /// 最小値
    /// </summary>
    private readonly decimal? _minValue;

    /// <summary>
    /// 最大値
    /// </summary>
    private readonly decimal? _maxValue;
    #endregion


    #region プロパティ
    /// <summary>
    /// 最小値テキスト
    /// </summary>
    public string MinValueText { get; }


    /// <summary>
    /// 最大値テキスト
    /// </summary>
    public string MaxValueText { get; }


    /// <inheritdoc/>
    public bool IsFilterEnabled => _minValue is not null || _maxValue is not null;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="minValueText">最小値</param>
    /// <param name="maxValueText">最大値</param>
    public NumericalBetweenContentFilter(string minValueText, string maxValueText)
    {
        if (decimal.TryParse(minValueText, out var minResult))
        {
            _minValue = minResult;
        }
        MinValueText = minValueText;

        if (decimal.TryParse(maxValueText, out var maxResult))
        {
            _maxValue = maxResult;
        }
        MaxValueText = maxValueText;
    }


    /// <inheritdoc/>
    public bool IsMatch(object? value)
    {
        // 最小値も最大値も未指定ならフィルタ無しと見なす
        if (_minValue is null && _maxValue is null)
        {
            return true;
        }

        // セルの値が空なら除外する
        if (value is null)
        {
            return false;
        }

        var val = Convert.ToDecimal(value);
        var ret = true;

        // 最小値以上か？
        if (_minValue is not null)
        {
            ret = _minValue <= val;
        }

        // 最大値以下か？
        if (ret && _maxValue is not null)
        {
            ret = val <= _maxValue;
        }

        return ret;
    }


    /// <inheritdoc/>
    public bool Equals(IContentFilter? other)
    {
        if (other is NumericalBetweenContentFilter filter)
        {
            return _minValue == filter._minValue && _maxValue == filter._maxValue;
        }

        return false;
    }
}
