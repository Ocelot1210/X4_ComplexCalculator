using System;
using System.Collections.Generic;

namespace X4_ComplexCalculator.Common
{
    class ProgressEx<T> : IProgress<T>
    {
        /// <summary>
        /// 現在値
        /// </summary>
        private T _Current;


        /// <summary>
        /// 進捗変更時に呼ばれるイベント
        /// </summary>
        public event EventHandler<T>? ProgressChanged;



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="firstValue">初期値</param>
        public ProgressEx(T firstValue)
        {
            _Current = firstValue;
        }


        /// <summary>
        /// 進捗変更
        /// </summary>
        /// <param name="value">変更後</param>
        public void Report(T value)
        {
            // 同じ値なら何もしない
            if (EqualityComparer<T>.Default.Equals(_Current, value))
            {
                return;
            }

            _Current = value;
            ProgressChanged?.Invoke(this, _Current);
        }
    }
}
