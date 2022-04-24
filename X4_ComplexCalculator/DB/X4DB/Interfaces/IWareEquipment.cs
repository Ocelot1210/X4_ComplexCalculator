using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// ウェアの装備情報用インターフェイス
/// </summary>
public interface IWareEquipment
{
    #region プロパティ
    /// <summary>
    /// ウェアID
    /// </summary>
    public string ID { get; }


    /// <summary>
    /// コネクション名
    /// </summary>
    public string ConnectionName { get; }


    /// <summary>
    /// 装備種別
    /// </summary>
    public IEquipmentType EquipmentType { get; }


    /// <summary>
    /// グループ名
    /// </summary>
    public string GroupName { get; }


    /// <summary>
    /// タグ情報
    /// </summary>
    public HashSet<string> Tags { get; }
    #endregion


    /// <summary>
    /// 指定した装備がthisに装備可能か判定する
    /// </summary>
    /// <param name="equipment">判定したい装備</param>
    /// <returns>指定した装備がthisに装備可能か</returns>
    public bool CanEquipped(IEquipment equipment);
}
