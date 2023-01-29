using Prism.Mvvm;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList;

/// <summary>
/// 兵装編集画面の装備品一覧1レコード分
/// </summary>
class EquipmentListItem : BindableBase
{
    #region メンバ
    /// <summary>
    /// 選択されているか
    /// </summary>
    private bool _isSelected;
    #endregion


    #region プロパティ
    /// <summary>
    /// 装備品
    /// </summary>
    public IEquipment Equipment { get; }


    /// <summary>
    /// 選択されているか
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="equipment">装備品</param>
    public EquipmentListItem(IEquipment equipment)
    {
        Equipment = equipment;
        _isSelected = false;
    }
}
