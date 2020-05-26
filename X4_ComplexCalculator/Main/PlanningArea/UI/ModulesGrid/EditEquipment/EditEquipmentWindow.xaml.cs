using System.Windows;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.PlanningArea.UI.ModulesGrid.EditEquipment
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

            var vm = new EditEquipmentViewModel(module);
            DataContext = vm;
            TurretsList.DataContext = vm.TurretsViewModel;
            ShieldsList.DataContext = vm.ShieldsViewModel;
        }
    }
}
