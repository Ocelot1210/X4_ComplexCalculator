using System;
using System.Collections.Generic;
using System.Text;

namespace X4_ComplexCalculator.Common.Reflection
{
    public interface IAccessor
    {
        /// <summary>
        /// 値の取得
        /// </summary>
        /// <param name="target">インスタンス</param>
        /// <returns>値</returns>
        object? GetValue(object target);


        /// <summary>
        /// 値の設定
        /// </summary>
        /// <param name="target">インスタンス</param>
        /// <param name="value">値</param>
        void SetValue(object target, object? value);
    }
}
