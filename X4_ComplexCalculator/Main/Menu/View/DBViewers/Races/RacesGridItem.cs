using CommunityToolkit.Mvvm.ComponentModel;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewers.Races;

/// <summary>
/// 種族表示用DataGridの1レコード分
/// </summary>
/// <param name="race">種族</param>
sealed partial class RacesGridItem(IRace race) : ObservableObject
{
    #region メンバ
    /// <summary>
    /// 種族
    /// </summary>
    private readonly IRace _race = race;
    #endregion


    #region プロパティ
    /// <summary>
    /// 種族ID
    /// </summary>
    public string ID => _race.RaceID;


    /// <summary>
    /// 名称
    /// </summary>
    public string Name => _race.Name;


    /// <summary>
    /// 略称
    /// </summary>
    public string ShortName => _race.ShortName;
    #endregion
}
