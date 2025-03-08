using CommunityToolkit.Mvvm.ComponentModel;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.SelectModule.Entities;


/// <summary>
/// コンストラクタ
/// </summary>
/// <param name="id">ID</param>
/// <param name="name">名称</param>
/// <param name="isChecked">チェック状態</param>
public sealed partial class ModulesListItem(string id, string name, bool isChecked) : ObservableObject
{
    /// <summary>
    /// モジュールID
    /// </summary>
    public string ID { get; } = id;


    /// <summary>
    /// モジュール名称
    /// </summary>
    public string Name { get; } = name;


    /// <summary>
    /// チェック状態
    /// </summary>
    [ObservableProperty]
    public partial bool IsChecked { get; set; } = isChecked;
}
