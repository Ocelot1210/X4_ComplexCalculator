using Prism.Mvvm;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StationSummary.WorkForce.NeedWareInfo;

/// <summary>
/// 必要ウェア詳細情報1レコード分
/// </summary>
class NeedWareInfoDetailsItem : BindableBase
{
    #region メンバ
    /// <summary>
    /// 必要数量
    /// </summary>
    private long _needAmount;


    /// <summary>
    /// 合計必要数量
    /// </summary>
    private long _totalNeedAmount;


    /// <summary>
    /// 生産数量
    /// </summary>
    private long _productionAmount;
    #endregion


    #region プロパティ
    /// <summary>
    /// 種族
    /// </summary>
    public IRace Race { get; }


    /// <summary>
    /// 労働方式
    /// </summary>
    public string Method { get; }


    /// <summary>
    /// ウェアID
    /// </summary>
    public string WareID { get; }


    /// <summary>
    /// 必要ウェア名
    /// </summary>
    public string WareName { get; }


    /// <summary>
    /// 必要数量
    /// </summary>
    public long NeedAmount
    {
        get => _needAmount;
        set
        {
            if (SetProperty(ref _needAmount, value))
            {
                RaisePropertyChanged(nameof(Diff));
            }
        }
    }


    /// <summary>
    /// 合計必要数量
    /// </summary>
    public long TotalNeedAmount
    {
        get => _totalNeedAmount;
        set => SetProperty(ref _totalNeedAmount, value);
    }


    /// <summary>
    /// 生産数量
    /// </summary>
    public long ProductionAmount
    {
        get => _productionAmount;
        set
        {
            if (SetProperty(ref _productionAmount, value))
            {
                RaisePropertyChanged(nameof(Diff));
            }
        }
    }


    /// <summary>
    /// 差
    /// </summary>
    public long Diff => ProductionAmount - NeedAmount;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="race">種族</param>
    /// <param name="method">労働方式</param>
    /// <param name="wareID">ウェアID</param>
    /// <param name="needAmount">必要数量</param>
    /// <param name="productionAmount">生産数量</param>
    public NeedWareInfoDetailsItem(IRace race, string method, string wareID, long needAmount = 0, long productionAmount = 0)
    {
        Race             = race;
        Method           = method;
        WareID           = wareID;
        WareName         = X4Database.Instance.Ware.Get(wareID).Name;
        NeedAmount       = needAmount;
        ProductionAmount = productionAmount;
    }
}
