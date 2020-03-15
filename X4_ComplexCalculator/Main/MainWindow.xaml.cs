using System.Windows.Data;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.Main.ModulesGrid;
using X4_ComplexCalculator.Main.ProductsGrid;
using X4_ComplexCalculator.Main.ResourcesGrid;
using X4_ComplexCalculator.Main.StoragesGrid;
using X4_ComplexCalculator.Main.StationSummary;
using X4_ComplexCalculator.Main;
using System;

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
            

            var moduleModel = new ModulesGridModel(this);
            var productsModel = new ProductsGridModel(moduleModel.Modules);
            var resourcesModel = new ResourcesGridModel(moduleModel.Modules);

            Summary.DataContext  = new StationSummaryViewModel(moduleModel.Modules);
            Modules.DataContext  = new ModulesGridViewModel(moduleModel, (CollectionViewSource)Modules.Resources["ModulesViewSource"]);
            Products.DataContext = new ProductsGridViewModel(productsModel);
            Build.DataContext    = new ResourcesGridViewModel(resourcesModel);
            Storages.DataContext = new StoragesGridViewModel(moduleModel.Modules);

            DataContext = new MainWindowViewModel(moduleModel, productsModel, resourcesModel);
        }
    }
}
