using System.Linq;
using System.Windows;

namespace X4_DataExporterWPF.DataExportWindow;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class DataExportWindow : Window
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="inDirPath">入力元フォルダパス</param>
    /// <param name="outFilePath">出力先ファイルパス</param>
    private DataExportWindow(string inDirPath, string outFilePath)
    {
        InitializeComponent();

        DataContext = new DataExportViewModel(inDirPath, outFilePath);
    }


    /// <summary>
    /// ダイアログを表示
    /// </summary>
    /// <param name="inDirPath"></param>
    /// <param name="outFilePath"></param>
    public static void ShowDialog(string inDirPath, string outFilePath)
    {
        var wnd = new DataExportWindow(inDirPath, outFilePath);

        wnd.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive) ?? Application.Current.MainWindow;

        wnd.ShowDialog();
    }
}
