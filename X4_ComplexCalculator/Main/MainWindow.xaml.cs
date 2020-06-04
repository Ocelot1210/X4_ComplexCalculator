using System;
using System.Windows;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.Main;

namespace X4_ComplexCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            if (WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.DefaultProvider is CSVLocalizationProvider provider)
            {
                provider.FileName = "Lang";
            }

            var conf = Configuration.GetConfiguration();
            var lang = conf["AppSettings:Language"];
            if (!string.IsNullOrEmpty(lang))
            {
                // 言語が設定されていればそれを使用
                WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo(lang);
            }
            else
            {
                // 言語が設定されていなければシステムのロケールを設定
                WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = System.Globalization.CultureInfo.CurrentUICulture;
            }

            try
            {
                DataContext = new MainWindowViewModel();
            }
            catch(Exception e)
            {
                X4_ComplexCalculator.Common.Localize.Localize.ShowMessageBox("Lang:UnexpectedErrorMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message, e.StackTrace ?? "");
                Environment.Exit(-1);
            }
        }
    }
}
