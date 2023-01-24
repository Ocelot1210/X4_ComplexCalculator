namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// 種族情報用インターフェイス
/// </summary>
public interface IRace
{
    #region プロパティ
    /// <summary>
    /// 種族ID
    /// </summary>
    public string RaceID { get; }


    /// <summary>
    /// 種族名
    /// </summary>
    public string Name { get; }


    /// <summary>
    /// 略称
    /// </summary>
    public string ShortName { get; }


    /// <summary>
    /// 説明文
    /// </summary>
    public string Description { get; }
    #endregion
}
