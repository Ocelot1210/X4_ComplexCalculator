using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// ウェア生産時の追加効果情報用クラス
/// </summary>
/// <param name="wareID">ウェアID</param>
/// <param name="method">生産方式</param>
/// <param name="effectID">追加効果ID</param>
/// <param name="product">追加効果の値</param>
public sealed record WareEffect(
    string WareID,
    string Method,
    string EffectID,
    double Product
) : IWareEffect;
