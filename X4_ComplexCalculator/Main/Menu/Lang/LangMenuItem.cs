using Prism.Mvvm;
using System;
using System.Collections.Generic;
using X4_ComplexCalculator.Common;
using System.Globalization;
using System.Text;
using WPFLocalizeExtension.Engine;

namespace X4_ComplexCalculator.Main.Menu.Lang
{
    public class LangMenuItem : BindableBase
    {
        /// <summary>
        /// 言語
        /// </summary>
        private CultureInfo _CultureInfo;


        /// <summary>
        /// チェックされたか
        /// </summary>
        private bool _IsChecked;


        /// <summary>
        /// チェックされたか
        /// </summary>
        public bool IsChecked
        {
            get => _IsChecked;
            set
            {
                if (SetProperty(ref _IsChecked, value))
                {
                    if (IsChecked)
                    {
                        LocalizeDictionary.Instance.Culture = _CultureInfo;

                        Configuration.SetValue("AppSettings.Language", _CultureInfo.Name);
                    }
                }
            }
        }


        /// <summary>
        /// 言語名
        /// </summary>
        public string Name => _CultureInfo.DisplayName;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="cultureInfo">言語情報</param>
        public LangMenuItem(CultureInfo cultureInfo)
        {
            _CultureInfo = cultureInfo;

            if (cultureInfo.Name == LocalizeDictionary.CurrentCulture.Name)
            {
                IsChecked = true;
            }
        }
    }
}
