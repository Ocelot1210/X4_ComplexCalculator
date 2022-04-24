namespace X4_DataExporterWPF.Entity;

/// <summary>
/// スラスター情報
/// </summary>
/// <param name="EquipmentID">装備ID</param>
/// <param name="ThrustStrafe">推進(水平・垂直)？</param>
/// <param name="ThrustPitch">推進(ピッチ)</param>
/// <param name="ThrustYaw">推進(ヨー)</param>
/// <param name="ThrustRoll">推進(ロール)</param>
/// <param name="AngularRoll">角度(ロール)？</param>
/// <param name="AngularPitch">角度(ピッチ)？</param>
public sealed record Thruster(
    string EquipmentID,
    double ThrustStrafe,
    double ThrustPitch,
    double ThrustYaw,
    double ThrustRoll,
    double AngularRoll,
    double AngularPitch
);
