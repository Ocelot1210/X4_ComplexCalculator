namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// 派閥情報用インターフェイス
/// </summary>
public interface IFaction
{
    #region プロパティ
    /// <summary>
    /// 派閥ID
    /// </summary>
    public string FactionID { get; }


    /// <summary>
    /// 派閥名
    /// </summary>
    public string Name { get; }


    /// <summary>
    /// 派閥の種族
    /// </summary>
    public IRace Race { get; }
    #endregion
}
