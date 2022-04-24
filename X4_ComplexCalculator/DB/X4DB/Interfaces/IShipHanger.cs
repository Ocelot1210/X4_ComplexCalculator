namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// 艦船のハンガー情報管理用インターフェイス
/// </summary>
public interface IShipHanger
{
    #region プロパティ
    /// <summary>
    /// 艦船ID
    /// </summary>
    public string ShipID { get; }


    /// <summary>
    /// 発着パッドのサイズ
    /// </summary>
    public IX4Size Size { get; }


    /// <summary>
    /// 発着パッド数
    /// </summary>
    public long Count { get; }


    /// <summary>
    /// 機体格納数
    /// </summary>
    public long Capacity { get; }
    #endregion
}
