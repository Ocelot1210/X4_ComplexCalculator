using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using X4_ComplexCalculator.Common;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

/// <summary>
/// ステーション設定用クラス
/// </summary>
public sealed partial class StationSettingInfo(IMessenger messenger) : ObservableRecipientEx(messenger, true)
{
    #region プロパティ
    /// <summary>
    /// 本部か
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial bool IsHeadquarters { get; set; }


    /// <summary>
    /// 本部の必要労働者数
    /// </summary>
    public int HQWorkers { get; } = 200;


    /// <summary>
    /// 労働者
    /// </summary>
    public WorkforceManager Workforce { get; } = new(messenger);


    /// <summary>
    /// 日光[%]
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial double Sunlight { get; set; }
    #endregion
}
