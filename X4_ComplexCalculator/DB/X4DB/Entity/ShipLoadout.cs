using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// 艦船のロードアウト情報用クラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="shipID">艦船ID</param>
/// <param name="loadoutID">ロードアウトID</param>
/// <param name="groupName">グループ名</param>
/// <param name="count">個数</param>
/// <param name="equipmentID">装備ID</param>
public sealed class ShipLoadout(string shipID, string loadoutID, string groupName, long count, string equipmentID) : IShipLoadout
{
    #region IShipLoadout
    /// <inheritdoc/>
    public string ID { get; } = shipID;


    /// <inheritdoc/>
    public string LoadoutID { get; } = loadoutID;


    /// <inheritdoc/>
    public IEquipment Equipment => X4Database.Instance.Ware.Get<IEquipment>(equipmentID);


    /// <inheritdoc/>
    public string GroupName { get; } = groupName;


    /// <inheritdoc/>
    public long Count { get; } = count;
    #endregion
}
