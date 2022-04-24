namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// 艦船のロードアウト情報用インターフェイス
/// </summary>
public interface IShipLoadout
{
    #region プロパティ
    /// <summary>
    /// 艦船ID
    /// </summary>
    public string ID { get; }


    /// <summary>
    /// ロードアウトID
    /// </summary>
    public string LoadoutID { get; }


    /// <summary>
    /// 装備品
    /// </summary>
    public IEquipment Equipment { get; }


    /// <summary>
    /// グループ名
    /// </summary>
    public string GroupName { get; }


    /// <summary>
    /// 個数
    /// </summary>
    public long Count { get; }
    #endregion
}
