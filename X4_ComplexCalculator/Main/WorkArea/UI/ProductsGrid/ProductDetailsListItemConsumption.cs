using Prism.Mvvm;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

/// <summary>
/// 製品一覧DataGridの＋/－で表示するListViewのアイテム(消費品)
/// </summary>
class ProductDetailsListItemConsumption : BindableBase, IProductDetailsListItem
{
    #region メンバ
    /// <summary>
    /// 製品数(モジュール追加用)
    /// </summary>
    private readonly long _amount;


    /// <summary>
    /// モジュール数
    /// </summary>
    private long _moduleCount;
    #endregion


    #region プロパティ
    /// <inheritdoc/>
    public string WareID { get; }


    /// <inheritdoc/>
    public string ModuleID { get; }


    /// <inheritdoc/>
    public string ModuleName { get; }


    /// <inheritdoc/>
    public long ModuleCount
    {
        get => _moduleCount;
        set
        {
            if (SetProperty(ref _moduleCount, value))
            {
                RaisePropertyChanged(nameof(Amount));
            }
        }
    }


    /// <inheritdoc/>
    public long Amount => _amount * ModuleCount;


    /// <inheritdoc/>
    public double Efficiency => -1.0;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="wareID">生産/消費ウェアID</param>
    /// <param name="module">モジュール</param>
    /// <param name="moduleCount">モジュール数</param>
    /// <param name="amount">製品数</param>
    public ProductDetailsListItemConsumption(string wareID, IX4Module module, long moduleCount, long amount)
    {
        WareID = wareID;
        ModuleID = module.ID;
        ModuleName = module.Name;
        ModuleCount = moduleCount;
        _amount = amount;
    }


    /// <summary>
    /// 生産性を設定
    /// </summary>
    /// <param name="effectID">効果ID</param>
    /// <param name="value">設定値</param>
    public void SetEfficiency(string effectID, double value)
    {
        // 何もしない
    }
}
