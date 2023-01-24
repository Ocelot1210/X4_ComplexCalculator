using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// 艦船のロードアウト情報用クラス
/// </summary>
public class ShipLoadout : IShipLoadout
{
    #region メンバ
    /// <summary>
    /// 装備品ID
    /// </summary>
    private readonly string _EquipmentID;
    #endregion


    #region IShipLoadout
    /// <inheritdoc/>
    public string ID { get; }


    /// <inheritdoc/>
    public string LoadoutID { get; }


    /// <inheritdoc/>
    public IEquipment Equipment => X4Database.Instance.Ware.Get<IEquipment>(_EquipmentID);


    /// <inheritdoc/>
    public string GroupName { get; }


    /// <inheritdoc/>
    public long Count { get; }
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="shipID">艦船ID</param>
    /// <param name="loadoutID">ロードアウトID</param>
    /// <param name="groupName">グループ名</param>
    /// <param name="count">個数</param>
    /// <param name="equipmentID">装備ID</param>
    public ShipLoadout(string shipID, string loadoutID, string groupName, long count, string equipmentID)
    {
        ID = shipID;
        LoadoutID = loadoutID;
        _EquipmentID = equipmentID;
        GroupName = groupName;
        Count = count;
    }
}
