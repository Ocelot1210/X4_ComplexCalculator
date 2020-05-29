using System.Linq;
using WPFLocalizeExtension.Engine;
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
