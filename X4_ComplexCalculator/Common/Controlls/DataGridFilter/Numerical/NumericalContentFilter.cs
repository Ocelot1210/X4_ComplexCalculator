using System;
using X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions;

namespace X4_ComplexCalculator.Common.Controlls.DataGridFilter.Numerical
{
    /// <summary>
    /// 数値フィルタ用クラス
    /// </summary>
    class NumericalContentFilter : IContentFilter
    {
        #region メンバ
        /// <summary>
        /// 選択値
        /// </summary>
        private readonly decimal? _Value;


        /// <summary>
        /// 一致条件
        /// </summary>
        private readonly NumericalFilterConditinos _Conditions;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="value">選択値</param>
        /// <param name="conditions">一致条件</param>
        public NumericalContentFilter(decimal? value, NumericalFilterConditinos conditions)
        {
            if (conditions == NumericalFilterConditinos.Between)
            {
                throw new NotSupportedException();
            }

            _Value = value;
            _Conditions = conditions;
        }


        /// <inheritdoc/>
        public bool IsMatch(object? value)
        {
            // 値が未指定ならフィルタ無しと見なす
            if (_Value is null)
            {
                return true;
            }

            // セルの値が空なら除外する
            if (value is null)
            {
                return false;
            }

            var val = Convert.ToDecimal(value);

            var ret = _Conditions switch
            {
                NumericalFilterConditinos.Equals =>               _Value == val,
                NumericalFilterConditinos.NotEquals =>            _Value != val,
                NumericalFilterConditinos.GreaterThan =>          _Value <  val,
                NumericalFilterConditinos.GreaterThanOrEqualTo => _Value <= val,
                NumericalFilterConditinos.LessThan =>             _Value >  val,
                NumericalFilterConditinos.LessThanOrEqualTo =>    _Value >= val,
                _ => throw new NotSupportedException(),
            };

            return ret;
        }
    }
}
