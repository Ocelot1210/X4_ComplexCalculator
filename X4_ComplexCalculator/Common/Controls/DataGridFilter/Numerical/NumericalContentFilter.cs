using System;
using X4_ComplexCalculator.Common.Controls.DataGridFilter.Interface;
using X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions;

namespace X4_ComplexCalculator.Common.Controls.DataGridFilter.Numerical;

/// <summary>
/// 数値フィルタ用クラス
/// </summary>
class NumericalContentFilter : IDataGridFilter
{
    #region メンバ
    /// <summary>
    /// 選択値
    /// </summary>
    private readonly decimal? _value;
    #endregion


    #region プロパティ
    /// <summary>
    /// 入力文字列
    /// </summary>
    public string InputText { get; }


    /// <summary>
    /// 一致条件
    /// </summary>
    public NumericalFilterConditinos Conditinos { get; }


    /// <inheritdoc/>
    public bool IsFilterEnabled => _value is not null;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="value">選択値の文字列</param>
    /// <param name="conditions">一致条件</param>
    public NumericalContentFilter(string text, NumericalFilterConditinos conditions)
    {
        if (conditions == NumericalFilterConditinos.Between)
        {
            throw new NotSupportedException();
        }

        if (decimal.TryParse(text, out var result))
        {
            _value = result;
        }
        InputText = text;
        Conditinos = conditions;
    }


    /// <inheritdoc/>
    public bool IsMatch(object? value)
    {
        // 値が無効ならフィルタ無しと見なす
        if (_value is null)
        {
            return true;
        }

        // セルの値が空なら除外する
        if (value is null)
        {
            return false;
        }

        var val = Convert.ToDecimal(value);

        var ret = Conditinos switch
        {
            NumericalFilterConditinos.Equals =>               _value == val,
            NumericalFilterConditinos.NotEquals =>            _value != val,
            NumericalFilterConditinos.GreaterThan =>          _value <  val,
            NumericalFilterConditinos.GreaterThanOrEqualTo => _value <= val,
            NumericalFilterConditinos.LessThan =>             _value >  val,
            NumericalFilterConditinos.LessThanOrEqualTo =>    _value >= val,
            _ => throw new NotSupportedException(),
        };

        return ret;
    }


    /// <inheritdoc/>
    public bool Equals(IContentFilter? other)
    {
        if (other is NumericalContentFilter filter)
        {
            return _value == filter._value && Conditinos == filter.Conditinos;
        }

        return false;
    }
}
