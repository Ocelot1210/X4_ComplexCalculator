using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// INotifyPropertyChangedを簡単に実装するためのクラス
    /// </summary>
    public class INotifyPropertyChangedBace : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// プロパティ変更時のイベントハンドラ
        /// </summary>
        private PropertyChangedEventHandler _PropertyChanged;

        private List<PropertyChangedEventHandler> _EventHandlers = new List<PropertyChangedEventHandler>();

        /// <summary>
        /// プロパティ変更時のイベントハンドラ
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                PropertyChangedEventHandler ev2;
                var ev1 = _PropertyChanged;
                do
                {
                    ev2 = ev1;
                    var ev3 = (PropertyChangedEventHandler)Delegate.Combine(ev2, value);
                    _EventHandlers.Add(value);
                    ev1 = Interlocked.CompareExchange(ref _PropertyChanged, ev3, ev2);
                }
                while (ev1 != ev2);
            }
            remove
            {
                PropertyChangedEventHandler ev2;
                var ev1 = _PropertyChanged;
                do
                {
                    ev2 = ev1;
                    var ev3 = (PropertyChangedEventHandler)Delegate.Remove(ev2, value);
                    _EventHandlers.Remove(value);
                    ev1 = Interlocked.CompareExchange(ref _PropertyChanged, ev3, ev2);
                }
                while (ev1 != ev2);
            }
        }


        /// <summary>
        /// プロパティ変更時
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            _PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public virtual void Dispose()
        {
            foreach(var ev in _EventHandlers.ToArray())
            {
                PropertyChanged -= ev;
            }
        }
    }
}
