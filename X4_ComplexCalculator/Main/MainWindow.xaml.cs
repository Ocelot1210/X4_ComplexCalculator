using System;
using X4_ComplexCalculator.DB;
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

            // DB接続  
            ContentRendered += (object sender, EventArgs e) => { DBConnection.Open(); };

            DataContext = new MainWindowViewModel();
        }
    }
}
