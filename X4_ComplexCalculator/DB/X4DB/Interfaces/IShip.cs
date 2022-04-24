using System.Collections.Generic;
using X4_ComplexCalculator.DB.X4DB.Entity;

namespace X4_ComplexCalculator.DB.X4DB.Interfaces;

/// <summary>
/// 艦船情報用インターフェース
/// </summary>
public interface IShip : IEquippableWare, IMacro
{
    #region プロパティ
    /// <summary>
    /// 艦船種別
    /// </summary>
    public IShipType ShipType { get; }


    /// <summary>
    /// 艦船サイズ
    /// </summary>
    public IX4Size Size { get; }


    /// <summary>
    /// 質量
    /// </summary>
    public double Mass { get; }


    /// <summary>
    /// 抗力
    /// </summary>
    public Drag Drag { get; }


    /// <summary>
    /// 慣性
    /// </summary>
    public Inertia Inertia { get; }


    /// <summary>
    /// 船体強度
    /// </summary>
    public long Hull { get; }


    /// <summary>
    /// 船員数
    /// </summary>
    public long People { get; }


    /// <summary>
    /// ミサイル搭載量
    /// </summary>
    public long MissileStorage { get; }


    /// <summary>
    /// ドローン搭載量
    /// </summary>
    public long DroneStorage { get; }


    /// <summary>
    /// カーゴサイズ
    /// </summary>
    public long CargoSize { get; }


    /// <summary>
    /// 艦船のハンガー情報
    /// </summary>
    public IReadOnlyDictionary<string, IShipHanger> ShipHanger { get; }


    /// <summary>
    /// ロードアウトIDをキーにしたロードアウト情報のディクショナリ
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<IShipLoadout>> Loadouts { get; }
    #endregion
}
