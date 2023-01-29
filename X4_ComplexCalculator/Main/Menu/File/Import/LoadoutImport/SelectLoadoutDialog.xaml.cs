using System.Linq;
using System.Windows;

namespace X4_ComplexCalculator.Main.Menu.File.Import.LoadoutImport;

/// <summary>
/// SelectLoadoutDialog.xaml の相互作用ロジック
/// </summary>
public partial class SelectLoadoutDialog : Window
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    private SelectLoadoutDialog()
    {
        InitializeComponent();
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
