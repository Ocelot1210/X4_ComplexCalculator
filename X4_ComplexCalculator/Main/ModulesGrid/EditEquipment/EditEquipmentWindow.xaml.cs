using System.Windows;
using System.Windows.Data;
using X4_ComplexCalculator.DB.X4DB;
using X4_ComplexCalculator.Main.ModulesGrid.EditEquipment.EquipmentList;

namespace X4_ComplexCalculator.Main.ModulesGrid.EditEquipment
{
    /// <summary>
    /// SelectModuleEquipmentWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditEquipmentWindow : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">モジュール</param>
        public EditEquipmentWindow(Module module)
        {
            InitializeComponent();

            var viewModel = new EditEquipmentViewModel(module);
            Closing += viewModel.OnWindowClosing;

            DataContext = viewModel;

            // タレット用タブの内容を作成
            {
                var tem = new TurretEquipmentListModel(module, viewModel);
                var cvs = (CollectionViewSource)TurretsList.Resources["EquipmentsViewSource"];
                var evm = new EquipmentListViewModel(tem, cvs);
                viewModel.OnSaveEquipment += evm.SaveEquipment;

                TurretsList.DataContext = evm;
            }

            // シールド用タブの内容を作成
            {
                var sem = new ShieldEquipmentListModel(module, viewModel);
                var cvs = (CollectionViewSource)ShieldsList.Resources["EquipmentsViewSource"];
                var evm = new EquipmentListViewModel(sem, cvs);
                viewModel.OnSaveEquipment += evm.SaveEquipment;

                ShieldsList.DataContext = evm;
            }
        }
    }
}
