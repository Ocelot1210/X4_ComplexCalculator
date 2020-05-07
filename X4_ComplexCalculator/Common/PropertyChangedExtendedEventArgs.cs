using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// PropertyChangedEventArgsの拡張版(変更前後の値も保持)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyChangedExtendedEventArgs<T> : PropertyChangedEventArgs
    {
        /// <summary>
        /// 変更前の値
        /// </summary>
        public virtual T OldValue { get; }

        /// <summary>
        /// 変更後の値
        /// </summary>
        public virtual T NewValue { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        /// <param name="oldValue">前回値</param>
        /// <param name="newValue">今回値</param>
        public PropertyChangedExtendedEventArgs(string propertyName, T oldValue, T newValue) : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
