using System.Windows;
using X4_ComplexCalculator.Common.Dialog.MessageBoxes;
using X4_ComplexCalculator.Entity;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

/// <summary>
/// SelectModuleEquipmentWindow.xaml の相互作用ロジック
/// </summary>
public partial class EditEquipmentWindow : Window
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="equipmentManager">編集対象の装備情報</param>
    public EditEquipmentWindow(EquippableWareEquipmentManager equipmentManager)
    {
        InitializeComponent();

        DataContext = new EditEquipmentViewModel(equipmentManager, new LocalizedMessageBoxEx(this));
    }
}
