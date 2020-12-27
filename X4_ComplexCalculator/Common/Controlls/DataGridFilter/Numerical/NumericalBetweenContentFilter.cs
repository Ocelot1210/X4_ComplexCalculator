using System;
using X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions;

namespace X4_ComplexCalculator.Common.Controlls.DataGridFilter.Numerical
{
    /// <summary>
    /// 数値フィルタ用クラス(指定の範囲内用)
    /// </summary>
    class NumericalBetweenContentFilter : IContentFilter
    {
        #region メンバ
        /// <summary>
        /// 最小値
        /// </summary>
        private readonly decimal? _MinValue;

        /// <summary>
        /// 最大値
        /// </summary>
        private readonly decimal? _MaxValue;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="minValue">最小値</param>
        /// <param name="maxValue">最大値</param>
        public NumericalBetweenContentFilter(decimal? minValue, decimal? maxValue)
        {
            _MinValue = minValue;
            _MaxValue = maxValue;
        }


        /// <inheritdoc/>
        public bool IsMatch(object? value)
        {
            // 最小値も最大値も未指定ならフィルタ無しと見なす
            if (_MinValue is null && _MaxValue is null)
            {
                return true;
            }

            // セルの値が空なら除外する
            if (value is null)
            {
                return false;
            }

            var val = Convert.ToDecimal(value);


            // 最小値以上かつ最大値以下か？
            return (_MaxValue is not null && _MinValue <= val) &&
                   (_MaxValue is not null && val <= _MaxValue);
        }
    }
}
