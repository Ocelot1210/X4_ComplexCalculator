using CommunityToolkit.Mvvm.Messaging;
using System.Windows;
using X4_ComplexCalculator.Common.Collections;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule;

/// <summary>
/// AddModuleWindow.xaml の相互作用ロジック
/// </summary>
public sealed partial class SelectModuleWindow : Window
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="messanger">メッセージ交換用</param>
    /// <param name="modules">モジュール追加対象</param>
    /// <param name="prevModuleName">変更前のモジュール</param>
    public SelectModuleWindow(IMessenger messenger, ObservableRangeCollection<ModulesGridItem> modules, string prevModuleName = "")
    {
        InitializeComponent();

        DataContext = new SelectModuleViewModel(messenger, modules, prevModuleName);
    }
}
