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
        DataContext = new MainWindowViewModel(new Common.Dialog.MessageBoxes.LocalizedMessageBoxEx(this));
    }
}
