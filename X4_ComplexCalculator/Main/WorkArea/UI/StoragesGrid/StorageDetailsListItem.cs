using CommunityToolkit.Mvvm.ComponentModel;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid;


/// <summary>
/// ドロップダウンで表示するListViewのアイテム(保管庫用)
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="module">モジュール</param>
/// <param name="moduleCount">モジュール数</param>
/// <param name="transportType">保管庫種別</param>
public sealed partial class StorageDetailsListItem(IX4Module module, long moduleCount, ITransportType transportType) : ObservableObject
{
    #region プロパティ
    /// <summary>
    /// モジュールID
    /// </summary>
    public string ModuleID => module.ID;


    /// <summary>
    /// モジュール名
    /// </summary>
    public string ModuleName => module.Name;


    /// <summary>
    /// 保管庫種別
    /// </summary>
    public ITransportType TransportType { get; } = transportType;



    /// <summary>
    /// モジュール数
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalCapacity))]
    public partial long ModuleCount { get; set; } = moduleCount;


    /// <summary>
    /// 保管庫容量
    /// </summary>
    public long Capacity { get; } = module.Storage.Amount / module.Storage.Types.Count;


    /// <summary>
    /// 保管庫容量(合計)
    /// </summary>
    public long TotalCapacity => Capacity * ModuleCount;
    #endregion
}
