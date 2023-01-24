using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Ships;

/// <summary>
/// 装備情報表示用
/// </summary>
public class EquipmentInfo<T> where T : class, IEquipment
{
    /// <summary>
    /// 表示する値
    /// </summary>
    public double Value { get; }


    /// <summary>
    /// ツールチップ文字列
    /// </summary>
    public string ToolTipText { get; }


    /// <summary>
    /// 装備と個数のタプルのリスト
    /// </summary>
    public IReadOnlyList<(T Equipment, int Count)> Equipments { get; }


    /// <summary>
    /// 速度用コンストラクタ
    /// </summary>
    /// <param name="engines">エンジン一覧</param>
    /// <param name="drag">抗力</param>
    /// <param name="thrustSelector">推進力選択用 Func</param>
    public EquipmentInfo(IEnumerable<IEngine> engines, double drag, Func<IEngine, double> thrustSelector)
    {
        Equipments = engines
            .GroupBy(x => x)
            .Select(x => (x.Key, x.Count()))
            .OrderBy(x => x.Key.Name)
            .Select(x => ((x.Key as T)!, x.Item2))
            .ToArray();

        Value = Math.Round(Equipments.Sum(x => thrustSelector((x.Equipment as IEngine)!) * x.Count) / drag, 1);

        ToolTipText = string.Join('\n', Equipments.Select(x => $"{x.Count} × {x.Equipment.Name}"));
    }


    /// <summary>
    /// 加速用コンストラクタ
    /// </summary>
    /// <param name="engines">エンジン一覧</param>
    /// <param name="mass">抗力</param>
    public EquipmentInfo(EquipmentInfo<IEngine> engines, double mass)
    {
        Equipments = engines.Equipments
            .Select(x => ((x.Equipment as T)!, x.Count))
            .ToArray();

        ToolTipText = engines.ToolTipText;

        Value = Math.Round(engines.Equipments.Sum(x => x.Equipment.Thrust.Forward * x.Count) / mass, 1);
    }


    /// <summary>
    /// ピッチ・ヨー・ロール 用コンストラクタ
    /// </summary>
    /// <param name="thrusters">スラスター一覧</param>
    /// <param name="drag">抗力</param>
    /// <param name="thrustSelector">推進力選択用 Func</param>
    public EquipmentInfo(IEnumerable<IThruster> thrusters, double drag, Func<IThruster, double> thrustSelector)
    {
        Equipments = thrusters
            .GroupBy(x => x)
            .Select(x => (x.Key, x.Count()))
            .OrderBy(x => x.Key.Name)
            .Select(x => ((x.Key as T)!, x.Item2))
            .ToArray();

        Value = Math.Round(Equipments.Sum(x => thrustSelector((x.Equipment as IThruster)!) * x.Count) / drag, 1);

        ToolTipText = string.Join('\n', Equipments.Select(x => $"{x.Count} × {x.Equipment.Name}"));
    }


    /// <summary>
    /// 平行移動速度用コンストラクタ
    /// </summary>
    /// <param name="thrusters">スラスター一覧</param>
    /// <param name="drag">抗力</param>
    public EquipmentInfo(IEnumerable<(IThruster Thruster, int Count)> thrusters, double drag)
    {
        Equipments = thrusters
            .Select(x => ((x.Thruster as T)!, x.Count))
            .ToArray();

        var totalThrust = Equipments.Sum(x => (x.Equipment as IThruster)!.ThrustStrafe * x.Count);
        Value = Math.Round(totalThrust / drag, 1);

        ToolTipText = string.Join('\n', Equipments.Select(x => $"{x.Count} × {x.Equipment.Name}"));
    }
}
