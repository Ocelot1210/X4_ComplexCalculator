using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity;

/// <summary>
/// 艦船情報情報用クラス
/// </summary>
public partial class Ship : IShip
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
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
    public Ship(
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
    )
    {
        ID = ware.ID;
        Name = ware.Name;
        WareGroup = ware.WareGroup;
        TransportType = ware.TransportType;
        Description = ware.Description;
        Volume = ware.Volume;
        MinPrice = ware.MinPrice;
        AvgPrice = ware.AvgPrice;
        MaxPrice = ware.MaxPrice;
        Owners = ware.Owners;
        Productions = ware.Productions;
        Resources = ware.Resources;
        Tags = ware.Tags;
        WareEffects = ware.WareEffects;

        ShipType = shipType;
        MacroName = macro;
        Size = size;
        Mass = mass;
        Drag = drag;
        Inertia = inertia;
        Hull = hull;
        People = people;
        MissileStorage = missileStorage;
        DroneStorage = droneStorage;
        CargoSize = cargoSize;
        ShipHanger = shipHanger;
        Loadouts = loadouts;
        Equipments = equipments;
    }


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
