using Prism.Mvvm;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;

/// <summary>
/// 保管庫容量情報
/// </summary>
public class StorageCapacityInfo : BindableBase
{
    #region メンバ
    /// <summary>
    /// 保管庫総容量
    /// </summary>
    private long _totalCapacity;


    /// <summary>
    /// 保管庫使用容量
    /// </summary>
    private long _usedCapacity;
    #endregion


    #region プロパティ
    /// <summary>
    /// 保管庫総容量
    /// </summary>
    public long TotalCapacity
    {
        get => _totalCapacity;
        set
        {
            if (SetProperty(ref _totalCapacity, value))
            {
                RaisePropertyChanged(nameof(FreeCapacity));
            }
        }
    }


    /// <summary>
    /// 保管庫使用容量
    /// </summary>
    public long UsedCapacity
    {
        get => _usedCapacity;
        set
        {
            if (SetProperty(ref _usedCapacity, value))
            {
                RaisePropertyChanged(nameof(FreeCapacity));
            }
        }
    }


    /// <summary>
    /// 保管庫空き容量
    /// </summary>
    public long FreeCapacity => TotalCapacity - _usedCapacity;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="totalCapacity">保管庫総容量</param>
    /// <param name="usedCapacity">保管庫使用容量</param>
    public StorageCapacityInfo(long totalCapacity = 0, long usedCapacity = 0)
    {
        TotalCapacity = totalCapacity;
        UsedCapacity = usedCapacity;
    }
}
