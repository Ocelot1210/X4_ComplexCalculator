using System.Windows;
using System.Windows.Data;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.ModulesGrid;
using X4_ComplexCalculator.Main.ProductsGrid;
using X4_ComplexCalculator.Main.ResourcesGrid;
using X4_ComplexCalculator.Main.StorageGrid;
using X4_ComplexCalculator.Main.StationSummary;

namespace X4_ComplexCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // DB接続
            DBConnection.Open();

            var moduleModel = new ModulesGridModel(this);

            Summary.DataContext     = new StationSummaryViewModel(moduleModel);
            Modules.DataContext     = new ModulesGridViewModel(moduleModel, (CollectionViewSource)Modules.Resources["ModulesViewSource"]);
            Products.DataContext    = new ProductsGridViewModel(moduleModel);
            Build.DataContext = new ResourcesGridViewModel(moduleModel);
            Storages.DataContext = new StoragesGridViewModel(moduleModel);
        }
    }
}
