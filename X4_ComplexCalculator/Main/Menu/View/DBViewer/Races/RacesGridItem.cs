using Prism.Mvvm;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Races;

/// <summary>
/// 種族表示用DataGridの1レコード分
/// </summary>
class RacesGridItem : BindableBase
{
    #region メンバ
    /// <summary>
    /// 種族
    /// </summary>
    private readonly IRace _Race;
    #endregion


    #region プロパティ
    /// <summary>
    /// 種族ID
    /// </summary>
    public string ID => _Race.RaceID;


    /// <summary>
    /// 名称
    /// </summary>
    public string Name => _Race.Name;


    /// <summary>
    /// 略称
    /// </summary>
    public string ShortName => _Race.ShortName;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="race">種族</param>
    public RacesGridItem(IRace race)
    {
        _Race = race;
    }
}
