using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// INotifyPropertyChangedを簡単に実装するためのクラス
    /// </summary>
    public class INotifyPropertyChangedBace : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティ変更時のイベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// プロパティ変更時
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// プロパティを設定
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="property">変更対象プロパティ</param>
        /// <param name="value">変更予定値</param>
        /// <param name="valueCheck">プロパティの値チェック処理</param>
        /// <param name="onChanged">プロパティを書き換えた場合のコールバック処理</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>プロパティを書き換えたか</returns>
        protected bool SetProperty<T>(ref T property, T value, Func<T, T, bool> valueCheck = null, Action onChanged = null, [CallerMemberName] string propertyName = "")
        {
            // 同じ値なら何もしない
            if (EqualityComparer<T>.Default.Equals(property, value))
            {
                return false;
            }

            // 値のチェックが通らなければ何もしない
            if (valueCheck != null && !valueCheck.Invoke(property, value))
            {
                return false;
            }

            property = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);

            return true;
        }
    }
}
