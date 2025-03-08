using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule.Entities;


/// <summary>
/// コンストラクタ
/// </summary>
/// <param name="id">ID</param>
/// <param name="name">名称</param>
/// <param name="isChecked">チェック状態</param>
public sealed partial class ModuleTypeListItem(IMessenger messenger, string id, string name, bool isChecked) : ObservableRecipientEx(messenger, true)
{
    /// <summary>
    /// モジュール種別ID
    /// </summary>
    public string ID { get; } = id;


    /// <summary>
    /// モジュール種別名
    /// </summary>
    public string Name { get; } = name;


    /// <summary>
    /// チェック状態
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial bool IsChecked { get; set; } = isChecked;
}