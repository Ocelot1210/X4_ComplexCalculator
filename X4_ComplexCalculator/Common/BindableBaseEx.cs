using Prism.Mvvm;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace X4_ComplexCalculator.Common
{
    public abstract class BindableBaseEx : BindableBase
    {

        /// <summary>
        /// 値が異なれば指定したプロパティに値を設定し、OnPropertyChangedを発火させる
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected bool SetPropertyEx<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            T prevValue = storage;
            storage = value;
            RaisePropertyChangedEx(prevValue, value, propertyName);

            return true;
        }


        /// <summary>
        /// プロパティ変更通知
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oldValue">前回値</param>
        /// <param name="newValue">今回値</param>
        /// <param name="propertyName">プロパティ名</param>
        protected void RaisePropertyChangedEx<T>(T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(new PropertyChangedExtendedEventArgs<T>(propertyName, oldValue, newValue));
        }
    }
}
