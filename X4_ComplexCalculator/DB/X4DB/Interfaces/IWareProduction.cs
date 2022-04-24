namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

public interface IWareProduction
{
    #region プロパティ
    /// <summary>
    /// ウェアID
    /// </summary>
    public string WareID { get; }


    /// <summary>
    /// 生産方式
    /// </summary>
    public string Method { get; }


    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; }


    /// <summary>
    /// 生産量
    /// </summary>
    public long Amount { get; }


    /// <summary>
    /// 生産時間
    /// </summary>
    public double Time { get; }
    #endregion
}
