namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// 装備種別情報用インターフェイス
/// </summary>
public interface IEquipmentType
{
    #region プロパティ
    /// <summary>
    /// 装備種別ID
    /// </summary>
    public string EquipmentTypeID { get; }


    /// <summary>
    /// 装備種別名
    /// </summary>
    public string Name { get; }
    #endregion
}
