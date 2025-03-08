using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// 艦船情報情報用クラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="ware">ウェア情報</param>
/// <param name="shipType">艦船種別</param>
/// <param name="macro">マクロ名</param>
/// <param name="size">サイズ</param>
/// <param name="mass">質量</param>
/// <param name="drag">抗力</param>
/// <param name="inertia">慣性</param>
/// <param name="hull">船体強度</param>
/// <param name="people">船員数</param>
/// <param name="missileStorage">ミサイル搭載量</param>
/// <param name="droneStorage">ドローン搭載量</param>
/// <param name="cargoSize">カーゴサイズ</param>
/// <param name="shipHanger">艦船のハンガー情報</param>
/// <param name="loadouts">ロードアウト情報</param>
/// <param name="equipments">装備一覧</param>
public sealed class Ship(
    IWare ware,
    IShipType shipType,
    string macro,
    IX4Size size,
    double mass,
    Drag drag,
    Inertia inertia,
    long hull,
    long people,
    long missileStorage,
    long droneStorage,
    long cargoSize,
    IReadOnlyDictionary<string, IShipHanger> shipHanger,
    IReadOnlyDictionary<string, IReadOnlyList<IShipLoadout>> loadouts,
    IReadOnlyDictionary<string, IWareEquipment> equipments
    ) : IShip
{
    #region IWare
    /// <inheritdoc/>
    public string ID { get; } = ware.ID;


    /// <inheritdoc/>
    public string Name { get; } = ware.Name;


    /// <inheritdoc/>
    public IWareGroup WareGroup { get; } = ware.WareGroup;


    /// <inheritdoc/>
    public ITransportType TransportType { get; } = ware.TransportType;


    /// <inheritdoc/>
    public string Description { get; } = ware.Description;


    /// <inheritdoc/>
    public long Volume { get; } = ware.Volume;


    /// <inheritdoc/>
    public long MinPrice { get; } = ware.MinPrice;


    /// <inheritdoc/>
    public long AvgPrice { get; } = ware.AvgPrice;


    /// <inheritdoc/>
    public long MaxPrice { get; } = ware.MaxPrice;


    /// <inheritdoc/>
    public IReadOnlyList<IFaction> Owners { get; } = ware.Owners;


    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IWareProduction> Productions { get; } = ware.Productions;


    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IReadOnlyList<IWareResource>> Resources { get; } = ware.Resources;


    /// <inheritdoc/>
    public HashSet<string> Tags { get; } = ware.Tags;


    /// <inheritdoc/>
    public IWareEffects WareEffects { get; } = ware.WareEffects;
    #endregion


    #region IEquippableWare
    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IWareEquipment> Equipments { get; } = equipments;
    #endregion


    #region IMacro
    /// <inheritdoc/>
    public string MacroName { get; } = macro;
    #endregion


    #region IShip
    /// <inheritdoc/>
    public IShipType ShipType { get; } = shipType;


    /// <inheritdoc/>
    public IX4Size Size { get; } = size;


    /// <inheritdoc/>
    public double Mass { get; } = mass;


    /// <inheritdoc/>
    public Drag Drag { get; } = drag;


    /// <inheritdoc/>
    public Inertia Inertia { get; } = inertia;


    /// <inheritdoc/>
    public long Hull { get; } = hull;


    /// <inheritdoc/>
    public long People { get; } = people;


    /// <inheritdoc/>
    public long MissileStorage { get; } = missileStorage;


    /// <inheritdoc/>
    public long DroneStorage { get; } = droneStorage;


    /// <inheritdoc/>
    public long CargoSize { get; } = cargoSize;


    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IShipHanger> ShipHanger { get; } = shipHanger;


    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IReadOnlyList<IShipLoadout>> Loadouts { get; } = loadouts;
    #endregion


    /// <summary>
    /// 指定したコネクション名に装備可能なEquipmentを取得する
    /// </summary>
    /// <returns></returns>
    public IEnumerable<T> GetEquippableEquipment<T>(string connectionName) where T : IEquipment
    {
        // 指定したコネクション名に装備可能な装備は存在するか？
        if (Equipments.TryGetValue(connectionName, out var wareEquipment))
        {
            // デフォルトのロードアウトは存在するか？
            if (Loadouts.TryGetValue("default", out var loadouts))
            {
                bool matched = false;

                // デフォルトのロードアウトの内、指定したコネクション名と同じグループ名を持つもので装備可能なものを取得する
                var shipLoadout = loadouts.FirstOrDefault(x => 
                    (x.GroupName == wareEquipment.GroupName && wareEquipment.CanEquipped(x.Equipment)) ||
                    (string.IsNullOrEmpty(x.GroupName) && wareEquipment.CanEquipped(x.Equipment))
                );
                if (shipLoadout is not null)
                {
                    // 同じグループ名の装備は指定した型と一致するか？
                    if (shipLoadout.Equipment is T ret)
                    {
                        matched = true;
                        yield return ret;
                    }
                }

                if (matched)
                {
                    yield break;
                }
            }


            var equipments = X4Database.Instance.Ware.GetAll<T>()
                .Where(x => wareEquipment.CanEquipped(x));
            foreach (var equipment in equipments)
            {
                yield return equipment;
            }
        }
    }
}
