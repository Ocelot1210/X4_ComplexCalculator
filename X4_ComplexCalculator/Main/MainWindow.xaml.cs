using System;
using System.Globalization;
using System.Windows;
using WPFLocalizeExtension.Engine;
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

            try
            {
                DataContext = new MainWindowViewModel();
            }
            catch(Exception e)
            {
                Common.Localize.LocalizedMessageBox.Show("Lang:UnexpectedErrorMessage", "Lang:Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, e.Message, e.StackTrace ?? "");
                Environment.Exit(-1);
            }
        }
    }
}
