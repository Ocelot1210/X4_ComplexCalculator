using System;
using System.Collections.Generic;
using System.Text;

namespace X4_ComplexCalculator.Common.Reflection
{
    internal sealed class Accessor<TTarget, TProperty> : IAccessor
    {
        private readonly Func<TTarget, TProperty> getter;
        private readonly Action<TTarget, TProperty> setter;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="getter"></param>
        /// <param name="setter"></param>
        public Accessor(Func<TTarget, TProperty> getter, Action<TTarget, TProperty> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        /// <summary>
        /// 値を設定
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public object GetValue(object target)
        {
            return getter((TTarget)target); ;
        }

        /// <summary>
        /// 値を取得
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public void SetValue(object target, object value)
        {
            setter((TTarget)target, (TProperty)value);
        }
    }
}
