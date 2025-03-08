using CommunityToolkit.Mvvm.ComponentModel;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ModulesGrid.EditEquipment;

/// <summary>
/// 派閥リストの1レコード分
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="faction">派閥</param>
/// <param name="isChecked">チェック状態</param>
sealed partial class FactionsListItem(IFaction faction, bool isChecked) : ObservableObject
{
    #region プロパティ
    /// <summary>
    /// 派閥
    /// </summary>
    public IFaction Faction { get; } = faction;


    /// <summary>
    /// 種族ID
    /// </summary>
    public string RaceID => Faction.Race.RaceID;


    /// <summary>
    /// 種族名
    /// </summary>
    public string RaceName => Faction.Race.Name;


    /// <summary>
    /// 派閥名
    /// </summary>
    public string FactionName => Faction.Name;


    /// <summary>
    /// チェック状態
    /// </summary>
    [ObservableProperty]
    public partial bool IsChecked { get; set; } = isChecked;
    #endregion
}
