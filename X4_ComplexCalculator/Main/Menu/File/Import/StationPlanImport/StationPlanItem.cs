using Prism.Mvvm;
using System.Xml.Linq;

namespace X4_ComplexCalculator.Main.Menu.File.Import.StationPlanImport;

/// <summary>
/// ステーション計画1レコード分
/// </summary>
public class StationPlanItem : BindableBase
{
    #region メンバ
    /// <summary>
    /// チェックされたか
    /// </summary>
    public bool _IsChecked;
    #endregion


    #region プロパティ
    /// <summary>
    /// 計画ID
    /// </summary>
    public string PlanID { get; }


    /// <summary>
    /// 計画名
    /// </summary>
    public string PlanName { get; }


    /// <summary>
    /// チェックされたか
    /// </summary>
    public bool IsChecked
    {
        get => _IsChecked;
        set => SetProperty(ref _IsChecked, value);
    }


    /// <summary>
    /// 計画
    /// </summary>
    public XElement Plan { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="planID">計画ID</param>
    /// <param name="planName">計画名</param>
    /// <param name="plan">計画を表す <see cref="XElement"/></param>
    public StationPlanItem(string planID, string planName, XElement plan)
    {
        PlanID = planID;
        PlanName = planName;
        Plan = plan;
    }
}
