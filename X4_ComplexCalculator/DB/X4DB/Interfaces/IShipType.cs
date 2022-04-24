namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// 艦船種別情報用インターフェイス
/// </summary>
public interface IShipType
{
    #region プロパティ
    /// <summary>
    /// 艦船種別ID
    /// </summary>
    public string ShipTypeID { get; }


    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; }


    /// <summary>
    /// 説明文
    /// </summary>
    public string Description { get; }
    #endregion
}
