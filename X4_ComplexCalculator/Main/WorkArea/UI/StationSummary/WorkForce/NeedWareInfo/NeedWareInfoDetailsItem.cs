using CommunityToolkit.Mvvm.ComponentModel;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.NeedWareInfo;


/// <summary>
/// 必要ウェア詳細情報1レコード分
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="race">種族</param>
/// <param name="method">労働方式</param>
/// <param name="wareID">ウェアID</param>
/// <param name="needAmount">必要数量</param>
/// <param name="productionAmount">生産数量</param>
sealed partial class NeedWareInfoDetailsItem(IRace race, string method, string wareID, long needAmount = 0, long productionAmount = 0) : ObservableObject
{
    #region プロパティ
    /// <summary>
    /// 種族
    /// </summary>
    public IRace Race { get; } = race;


    /// <summary>
    /// 労働方式
    /// </summary>
    public string Method { get; } = method;


    /// <summary>
    /// ウェアID
    /// </summary>
    public string WareID { get; } = wareID;


    /// <summary>
    /// 必要ウェア名
    /// </summary>
    public string WareName { get; } = X4Database.Instance.Ware.Get(wareID).Name;


    /// <summary>
    /// 必要数量
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Diff))]
    public partial long NeedAmount { get; set; } = needAmount;


    /// <summary>
    /// 合計必要数量
    /// </summary>
    [ObservableProperty]
    public partial long TotalNeedAmount { get; set; }


    /// <summary>
    /// 生産数量
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Diff))]
    public partial long ProductionAmount { get; set; } = productionAmount;


    /// <summary>
    /// 差
    /// </summary>
    public long Diff => ProductionAmount - NeedAmount;
    #endregion
}
