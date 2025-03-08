using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;

/// <summary>
/// 保管庫容量情報
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="totalCapacity">保管庫総容量</param>
/// <param name="usedCapacity">保管庫使用容量</param>
public sealed partial class StorageCapacityInfo(IMessenger messenger, long totalCapacity = 0, long usedCapacity = 0) : ObservableRecipientEx(messenger, true)
{
    #region プロパティ
    /// <summary>
    /// 保管庫総容量
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FreeCapacity))]
    public partial long TotalCapacity { get; set; } = totalCapacity;


    /// <summary>
    /// 保管庫使用容量
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FreeCapacity))]
    public partial long UsedCapacity { get; set; } = usedCapacity;


    /// <summary>
    /// 保管庫空き容量
    /// </summary>
    public long FreeCapacity => TotalCapacity - UsedCapacity;
    #endregion


    /// <summary>
    /// <see cref="TotalCapacity"/> 変更時
    /// </summary>
    partial void OnTotalCapacityChanged(long oldValue, long newValue)
    {
        Broadcast(oldValue - UsedCapacity, FreeCapacity, nameof(FreeCapacity));
    }


    /// <summary>
    /// <see cref="UsedCapacity"/> 変更時
    /// </summary>
    partial void OnUsedCapacityChanged(long oldValue, long newValue)
    {
        Broadcast(TotalCapacity - oldValue, FreeCapacity, nameof(FreeCapacity));
    }
}
