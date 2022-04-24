using Prism.Mvvm;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StoragesGrid;

/// <summary>
/// ドロップダウンで表示するListViewのアイテム(保管庫用)
/// </summary>
public class StorageDetailsListItem : BindableBase
{
    #region メンバ
    /// <summary>
    /// モジュール
    /// </summary>
    private readonly IX4Module _Module;


    /// <summary>
    /// モジュール数
    /// </summary>
    private long _ModuleCount;
    #endregion


    #region プロパティ
    /// <summary>
    /// モジュールID
    /// </summary>
    public string ModuleID => _Module.ID;


    /// <summary>
    /// モジュール名
    /// </summary>
    public string ModuleName => _Module.Name;


    /// <summary>
    /// 保管庫種別
    /// </summary>
    public ITransportType TransportType { get; }



    /// <summary>
    /// モジュール数
    /// </summary>
    public long ModuleCount
    {
        get => _ModuleCount;
        set
        {
            if (SetProperty(ref _ModuleCount, value))
            {
                RaisePropertyChanged(nameof(TotalCapacity));
            }
        }
    }


    /// <summary>
    /// 保管庫容量
    /// </summary>
    public long Capacity { get; }


    /// <summary>
    /// 保管庫容量(合計)
    /// </summary>
    public long TotalCapacity => Capacity * ModuleCount;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="module">モジュール</param>
    /// <param name="moduleCount">モジュール数</param>
    /// <param name="transportType">保管庫種別</param>
    public StorageDetailsListItem(IX4Module module, long moduleCount, ITransportType transportType)
    {
        _Module = module;
        ModuleCount = moduleCount;
        Capacity = module.Storage.Amount / module.Storage.Types.Count;
        TransportType = transportType;
    }
}
