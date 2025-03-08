using CommunityToolkit.Mvvm.ComponentModel;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid;

/// <summary>
/// 製品一覧DataGridの＋/－で表示するListViewのアイテム(消費品)
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="wareID">生産/消費ウェアID</param>
/// <param name="module">モジュール</param>
/// <param name="moduleCount">モジュール数</param>
/// <param name="amount">製品数</param>
sealed partial class ProductDetailsListItemConsumption(string wareID, IX4Module module, long moduleCount, long amount) : ObservableObject, IProductDetailsListItem
{
    #region プロパティ
    /// <inheritdoc/>
    public string WareID { get; } = wareID;


    /// <inheritdoc/>
    public string ModuleID { get; } = module.ID;


    /// <inheritdoc/>
    public string ModuleName { get; } = module.Name;


    /// <inheritdoc/>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Amount))]
    public partial long ModuleCount { get; set; } = moduleCount;


    /// <inheritdoc/>
    public long Amount => amount * ModuleCount;


    /// <inheritdoc/>
    public double Efficiency => -1.0;
    #endregion


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
