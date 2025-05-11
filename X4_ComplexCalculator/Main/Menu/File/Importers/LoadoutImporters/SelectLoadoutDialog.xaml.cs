using System.Linq;
using System.Windows;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;

namespace X4_ComplexCalculator.Main.Menu.File.Importers.LoadoutImporters;

/// <summary>
/// SelectLoadoutDialog.xaml の相互作用ロジック
/// </summary>
public sealed partial class SelectLoadoutDialog : Window
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    private SelectLoadoutDialog()
    {
        InitializeComponent();

        DataContext = new LoadoutImporterViewModel(new LocalizedMessageBoxEx(this));
    }


    /// <summary>
    /// ダイアログ表示
    /// </summary>
    public static bool ShowImportDialog()
    {
        var wnd = new SelectLoadoutDialog
        {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive) ?? Application.Current.MainWindow
        };

        return wnd.ShowDialog() == true;
    }
}
