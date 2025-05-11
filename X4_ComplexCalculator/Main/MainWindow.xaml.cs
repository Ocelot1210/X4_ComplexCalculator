using CommunityToolkit.Mvvm.Messaging;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;

namespace X4_ComplexCalculator.Main;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow
{
    public MainWindow()
    {
        System.Windows.Forms.Application.EnableVisualStyles();

        InitializeComponent();
        DataContext = new MainWindowViewModel(WeakReferenceMessenger.Default, new LocalizedMessageBoxEx(this));
    }
}
