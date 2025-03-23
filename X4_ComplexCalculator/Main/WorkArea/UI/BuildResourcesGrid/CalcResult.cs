namespace X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;

/// <summary>
/// 建造リソースの計算結果
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="wareID">建造に必要なウェアID</param>
/// <param name="amount">必要量</param>
public sealed class CalcResult(string wareID, long amount)
{
    #region プロパティ
    /// <summary>
    /// 建造に必要なウェアID
    /// </summary>
    public string WareID { get; } = wareID;


    /// <summary>
    /// 必要量
    /// </summary>
    public long Amount { get; } = amount;
    #endregion
}
