using X4_ComplexCalculator.DB.X4DB.Entity;

namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// エンジン情報用インターフェース
/// </summary>
public interface IEngine : IEquipment
{
    #region プロパティ
    /// <summary>
    /// 推進力
    /// </summary>
    public EngineThrust Thrust { get; }


    /// <summary>
    /// ブースト持続時間
    /// </summary>
    public double BoostDuration { get; }


    /// <summary>
    /// ブースト解除時間
    /// </summary>
    public double BoostReleaseTime { get; }


    /// <summary>
    /// トラベル解除時間
    /// </summary>
    public double TravelReleaseTime { get; }
    #endregion
}
