


using Prism.Mvvm;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

/// <summary>
/// プリセットコンボボックス用アイテム
/// </summary>
class PresetComboboxItem : BindableBase
{
    #region メンバ
    /// <summary>
    /// プリセット名
    /// </summary>
    private string _name;
    #endregion


    #region プロパティ
    /// <summary>
    /// プリセットID
    /// </summary>
    public long ID { get; }


    /// <summary>
    /// プリセット名
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">プリセットID(内部用)</param>
    /// <param name="name">プリセット名</param>
    public PresetComboboxItem(long id, string name)
    {
        ID = id;
        _name = name;
    }
}
