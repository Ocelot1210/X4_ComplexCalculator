using CommunityToolkit.Mvvm.ComponentModel;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment.EquipmentList;

/// <summary>
/// 兵装編集画面の装備品一覧1レコード分
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="equipment">装備品</param>
sealed partial class EquipmentListItem(IEquipment equipment) : ObservableObject
{
    #region プロパティ
    /// <summary>
    /// 装備品
    /// </summary>
    public IEquipment Equipment { get; } = equipment;


    /// <summary>
    /// 選択されているか
    /// </summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }
    #endregion
}
