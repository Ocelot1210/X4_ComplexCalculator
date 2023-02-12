using X4_ComplexCalculator.Common.Dialog.MessageBoxes;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        System.Windows.Forms.Application.EnableVisualStyles();

        InitializeComponent();
        DataContext = new MainWindowViewModel(new LocalizedMessageBoxEx(this));
    }
}
