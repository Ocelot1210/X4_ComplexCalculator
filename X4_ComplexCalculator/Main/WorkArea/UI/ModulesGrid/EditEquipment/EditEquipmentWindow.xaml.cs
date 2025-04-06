using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using X4_ComplexCalculator.Common.Dialogs.MessageBoxes;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

/// <summary>
/// SelectModuleEquipmentWindow.xaml の相互作用ロジック
/// </summary>
public sealed partial class EditEquipmentWindow : Window
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messenger">メッセージ通知用</param>
    /// <param name="equipmentManager">編集対象の装備情報</param>
    public EditEquipmentWindow(IMessenger messenger, EquippableWareEquipmentManager equipmentManager)
    {
        InitializeComponent();

        DataContext = new EditEquipmentViewModel(messenger, equipmentManager, new LocalizedMessageBoxEx(this));
    }
}
