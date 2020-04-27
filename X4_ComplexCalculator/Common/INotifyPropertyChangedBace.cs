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
    }
}
