namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// 艦船の慣性情報用クラス
/// </summary>
/// <param name="Pitch">ピッチ</param>
/// <param name="Yaw">ヨー</param>
/// <param name="Roll">ロール</param>
public sealed record Inertia(double Pitch, double Yaw, double Roll);
