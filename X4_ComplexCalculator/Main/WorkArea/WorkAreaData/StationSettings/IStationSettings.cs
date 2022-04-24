using System.ComponentModel;

namespace X4_ComplexCalculator.Main.WorkArea.WorkAreaData.StationSettings;

/// <summary>
/// ステーション設定情報用インターフェイス
/// </summary>
public interface IStationSettings : INotifyPropertyChanged
{
    /// <summary>
    /// 本部か
    /// </summary>
    public bool IsHeadquarters { get; set; }


    /// <summary>
    /// 本部の必要労働者数
    /// </summary>
    public int HQWorkers { get; }


    /// <summary>
    /// 労働者
    /// </summary>
    public WorkforceManager Workforce { get; }


    /// <summary>
    /// 日光[%]
    /// </summary>
    public double Sunlight { get; set; }
}
