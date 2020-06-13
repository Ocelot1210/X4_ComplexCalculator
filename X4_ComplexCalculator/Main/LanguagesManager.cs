using System;
using System.Globalization;
using System.Linq;
using WPFLocalizeExtension.Engine;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.Main.Menu.Lang;

namespace X4_ComplexCalculator.Main
{
    /// <summary>
    /// 言語一覧管理用クラス
    /// </summary>
    class LanguagesManager
    {
        /// <summary>
        /// 言語一覧
        /// </summary>
        public ObservablePropertyChangedCollection<LangMenuItem> Languages = new ObservablePropertyChangedCollection<LangMenuItem>();


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LanguagesManager()
        {
            if (LocalizeDictionary.Instance.DefaultProvider is CSVLocalizationProvider provider)
            {
                provider.FileName = "Lang";
            }

            var config = Configuration.GetConfiguration();
            var lang = config["AppSettings:Language"];
            
            try
            {
                // 言語が設定されているか？
                if (!string.IsNullOrWhiteSpace(lang))
                {
                    // 言語が設定されていればそれを使用
                    LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo(lang);
                }
                else
                {
                    // 言語が設定されていない場合、システムのロケールを設定
                    LocalizeDictionary.Instance.Culture = CultureInfo.CurrentUICulture;
                }
            }
            catch (ArgumentException e) when (e is ArgumentNullException || e is CultureNotFoundException)
            {
                // 無効な言語が指定されている場合はシステムのロケールを設定
                LocalizeDictionary.Instance.Culture = CultureInfo.CurrentUICulture;
            }

            Languages.AddRange(LocalizeDictionary.Instance.DefaultProvider.AvailableCultures.Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => new LangMenuItem(x)));
            Languages.CollectionPropertyChanged += Languages_CollectionPropertyChanged;
        }


        /// <summary>
        /// 言語一覧のプロパティ変更時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Languages_CollectionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!(sender is LangMenuItem langMenuItem))
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(LangMenuItem.IsChecked):
                    {
                        if (langMenuItem.IsChecked)
                        {
                            foreach (var lang in Languages)
                            {
                                if (!ReferenceEquals(lang, langMenuItem))
                                {
                                    lang.IsChecked = false;
                                }
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
