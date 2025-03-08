using CommunityToolkit.Mvvm.ComponentModel;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

/// <summary>
/// プリセットコンボボックス用アイテム
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="id">プリセットID(内部用)</param>
/// <param name="name">プリセット名</param>
sealed partial class PresetComboboxItem(long id, string name) : ObservableObject
{
    #region プロパティ
    /// <summary>
    /// プリセットID
    /// </summary>
    public long ID { get; } = id;


    /// <summary>
    /// プリセット名
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = name;
    #endregion
}
