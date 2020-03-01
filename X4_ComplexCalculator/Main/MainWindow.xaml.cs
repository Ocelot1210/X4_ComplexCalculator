using System.Windows;
using System.Windows.Data;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.ModulesGrid;
using X4_ComplexCalculator.Main.ProductsGrid;
using X4_ComplexCalculator.Main.ResourcesGrid;
using X4_ComplexCalculator.Main.StorageGrid;

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

            modulesGrid.DataContext = new ModulesGridViewModel(moduleModel, (CollectionViewSource)modulesGrid.Resources["ModulesViewSource"]);
            productsGrid.DataContext = new ProductsGridViewModel(moduleModel);
            resourcesGrid.DataContext = new ResourcesGridViewModel(moduleModel);
            storagesGrid.DataContext = new StoragesGridViewModel(moduleModel);
        }
    }
}
