namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// スラスター情報用インターフェース
/// </summary>
public interface IThruster : IEquipment
{
    #region プロパティ
    /// <summary>
    /// 推進(水平・垂直)？
    /// </summary>
    public double ThrustStrafe { get; }


    /// <summary>
    /// 推進力(ピッチ)
    /// </summary>
    public double ThrustPitch { get; }


    /// <summary>
    /// 推進力(ヨー)
    /// </summary>
    public double ThrustYaw { get; }


    /// <summary>
    /// 推進力(ロール)
    /// </summary>
    public double ThrustRoll { get; }


    /// <summary>
    /// 角度(ロール)？
    /// </summary>
    public double AngularRoll { get; }


    /// <summary>
    /// 角度(ピッチ)？
    /// </summary>
    public double AngularPitch { get; }
    #endregion
}
