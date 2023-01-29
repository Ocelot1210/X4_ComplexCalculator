using System;
using System.Collections.Generic;

namespace X4_ComplexCalculator.Common;

class ProgressEx<T> : IProgress<T>
{
    /// <summary>
    /// 現在値
    /// </summary>
    private T _current;


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
        _current = firstValue;
    }


    /// <summary>
    /// 進捗変更
    /// </summary>
    /// <param name="value">変更後</param>
    public void Report(T value)
    {
        // 同じ値なら何もしない
        if (EqualityComparer<T>.Default.Equals(_current, value))
        {
            return;
        }

        _current = value;
        ProgressChanged?.Invoke(this, _current);
    }
}
