namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// 艦船の抗力情報用クラス
/// </summary>
/// <param name="Forward">前方抗力</param>
/// <param name="Reverse">後方抗力</param>
/// <param name="Horizontal">水平抗力</param>
/// <param name="Vertical">垂直抗力</param>
/// <param name="Pitch">ピッチ抗力</param>
/// <param name="Yaw">ヨー抗力</param>
/// <param name="Roll">ロール抗力</param>
public sealed record Drag(
    double Forward,
    double Reverse,
    double Horizontal,
    double Vertical,
    double Pitch,
    double Yaw,
    double Roll
);
