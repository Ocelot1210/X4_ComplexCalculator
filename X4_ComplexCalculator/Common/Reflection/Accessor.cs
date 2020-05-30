using System;

namespace X4_ComplexCalculator.Common.Reflection
{
    internal sealed class Accessor<TTarget, TProperty> : IAccessor
    {
        private readonly Func<TTarget, TProperty> _Getter;
        private readonly Action<TTarget, TProperty> _Setter;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="getter"></param>
        /// <param name="setter"></param>
        public Accessor(Func<TTarget, TProperty> getter, Action<TTarget, TProperty> setter)
        {
            _Getter = getter;
            _Setter = setter;
        }

        /// <summary>
        /// 値を取得
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public object? GetValue(object target)
        {
            return _Getter((TTarget)target);
        }

        /// <summary>
        /// 値を設定
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public void SetValue(object target, object? value)
        {
            if (value != null)
            {
                _Setter((TTarget)target, (TProperty)value);
            }
        }
    }
}
