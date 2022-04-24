namespace X4_ComplexCalculator.Main.WorkArea.UI.BuildResourcesGrid;

/// <summary>
/// 建造リソースの計算結果
/// </summary>
public class CalcResult
{
    #region プロパティ
    /// <summary>
    /// 建造に必要なウェアID
    /// </summary>
    public string WareID { get; }


    /// <summary>
    /// 必要量
    /// </summary>
    public long Amount { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="wareID">建造に必要なウェアID</param>
    /// <param name="amount">必要量</param>
    public CalcResult(string wareID, long amount)
    {
        WareID = wareID;
        Amount = amount;
    }
}
