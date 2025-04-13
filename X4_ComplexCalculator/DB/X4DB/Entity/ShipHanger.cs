using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// 艦船のハンガー情報用クラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="shipID">艦船ID</param>
/// <param name="sizeID">発着パッドのサイズID</param>
/// <param name="count">発着パッド数</param>
/// <param name="capacity">機体格納数</param>
public sealed class ShipHanger(string shipID, string sizeID, long count, long capacity) : IShipHanger
{
    #region IShipHanger
    /// <inheritdoc/>
    public string ShipID { get; } = shipID;


    /// <inheritdoc/>
    public IX4Size Size { get; } = X4Database.Instance.X4Size.Get(sizeID);


    /// <inheritdoc/>
    public long Count { get; } = count;


    /// <inheritdoc/>
    public long Capacity { get; } = capacity;
    #endregion
}
