using CommunityToolkit.Mvvm.ComponentModel;
using System.Xml.Linq;

namespace X4_ComplexCalculator.Main.Menu.File.Importers.StationPlanImporters;

/// <summary>
/// ステーション計画1レコード分
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="planID">計画ID</param>
/// <param name="planName">計画名</param>
/// <param name="plan">計画を表す <see cref="XElement"/></param>
public sealed partial class StationPlanItem(string planID, string planName, XElement plan) : ObservableObject
{
    #region プロパティ
    /// <summary>
    /// 計画ID
    /// </summary>
    public string PlanID { get; } = planID;


    /// <summary>
    /// 計画名
    /// </summary>
    public string PlanName { get; } = planName;


    /// <summary>
    /// チェックされたか
    /// </summary>
    [ObservableProperty]
    public partial bool IsChecked { get; set; }


    /// <summary>
    /// 計画
    /// </summary>
    public XElement Plan { get; } = plan;
    #endregion
}
