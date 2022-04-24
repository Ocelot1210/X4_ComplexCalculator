namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// シールド情報用インターフェース
/// </summary>
interface IShield : IEquipment
{
    #region プロパティ
    /// <summary>
    /// 最大シールド容量
    /// </summary>
    public long Capacity { get; }


    /// <summary>
    /// 再充電率
    /// </summary>
    public long RechargeRate { get; }


    /// <summary>
    /// 再充電遅延
    /// </summary>
    public double RechargeDelay { get; }
    #endregion
}
