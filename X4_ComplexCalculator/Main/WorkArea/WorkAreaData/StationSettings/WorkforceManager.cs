using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Runtime.CompilerServices;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

/// <summary>
/// 労働者数を管理するクラス
/// </summary>
public sealed partial class WorkforceManager(IMessenger messenger) : ObservableRecipientEx(messenger, true)
{
    #region プロパティ
    /// <summary>
    /// 現在の労働者数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    [NotifyPropertyChangedFor(nameof(Proportion))]
    public partial long Actual { get; set; }


    /// <summary>
    /// 必要な労働者数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    [NotifyPropertyChangedFor(nameof(Proportion))]
    public partial long Need { get; set; }


    /// <summary>
    /// 収容人数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial long Capacity { get; set; }


    /// <summary>
    /// 現在の労働者数と必要な労働者数の割合
    /// </summary>
    public double Proportion => CalcProportion(Actual, Need);


    /// <summary>
    /// 常に最大にするか
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial bool AlwaysMaximum { get; set; }
    #endregion


    /// <summary>
    /// 内容をクリアする
    /// </summary>
    public void Clear()
    {
        Need = 0;
        Capacity = 0;
        Actual = 0;
    }


    /// <summary>
    /// 現在の労働者数変更時
    /// </summary>
    partial void OnActualChanged(long oldValue, long newValue) => Broadcast(CalcProportion(oldValue, Need), Proportion, nameof(Proportion));


    /// <summary>
    /// 必要な労働者数変更時
    /// </summary>
    partial void OnNeedChanged(long oldValue, long newValue) => Broadcast(CalcProportion(Actual, oldValue), Proportion, nameof(Proportion));


    /// <summary>
    /// 常に最大にするかが変更時
    /// </summary>
    /// <remarks>常に最大なら <see cref="Actual"/> を更新する</remarks>
    partial void OnAlwaysMaximumChanged(bool value) => Actual = value ? Capacity : Actual;


    /// <summary>
    /// 収容人数変更時
    /// </summary>
    partial void OnCapacityChanged(long value) => Actual = (value < Actual || Actual < value && AlwaysMaximum) ? Capacity : Actual;


    /// <summary>
    /// 現在の労働者数と必要な労働者数の割合を計算
    /// </summary>
    /// <param name="actual">現在の労働者数</param>
    /// <param name="need">必要な労働者数</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double CalcProportion(long actual, long need) => need == 0 ? 0.0 : (double)actual / need;
}
