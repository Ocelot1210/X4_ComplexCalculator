using System.Windows;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment
{
    /// <summary>
    /// SelectModuleEquipmentWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditEquipmentWindow : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">編集対象モジュール</param>
        public EditEquipmentWindow(ModulesGridItem module)
        {
            InitializeComponent();

            DataContext = new EditEquipmentViewModel(module);
        }
    }
}
