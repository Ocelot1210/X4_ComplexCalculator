namespace X4_DataExporterWPF.Entity;

/// <summary>
/// エンジン情報
/// </summary>
/// <param name="EquipmentID">装備ID</param>
/// <param name="ForwardThrust">前方推進力</param>
/// <param name="ReverseThrust">後方推進力</param>
/// <param name="BoostThrust">ブースト推進力</param>
/// <param name="BoostDuration">ブースト持続時間</param>
/// <param name="BoostReleaseTime">ブースト解除時間</param>
/// <param name="TravelThrust">トラベル推進力</param>
/// <param name="TravelReleaseTime">トラベル解除時間</param>
public sealed record Engine(
    string EquipmentID,
    double ForwardThrust,
    double ReverseThrust,
    double BoostThrust,
    double BoostDuration,
    double BoostReleaseTime,
    double TravelThrust,
    double TravelReleaseTime
);
