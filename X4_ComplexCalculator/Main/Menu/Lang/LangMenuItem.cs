using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Globalization;

namespace X4_ComplexCalculator.Main.Menu.Lang
{
    /// <summary>
    /// 言語メニュー1レコード分
    /// </summary>
    public class LangMenuItem : BindableBase
    {
        #region プロパティ
        /// <summary>
        /// 言語
        /// </summary>
        public CultureInfo CultureInfo { get; }


        /// <summary>
        /// チェックされたか
        /// </summary>
        public ReactivePropertySlim<bool> IsChecked { get; }


        /// <summary>
        /// チェックを外すことはできない
        /// </summary>
        public ReadOnlyReactivePropertySlim<bool> IsCheckable { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="cultureInfo">言語情報</param>
        public LangMenuItem(CultureInfo cultureInfo, bool isChecked)
        {
            CultureInfo = cultureInfo;
            IsChecked = new ReactivePropertySlim<bool>(isChecked);
            IsCheckable = IsChecked.Inverse().ToReadOnlyReactivePropertySlim();
        }
    }
}
