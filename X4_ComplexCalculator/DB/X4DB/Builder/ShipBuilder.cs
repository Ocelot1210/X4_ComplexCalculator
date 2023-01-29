using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.DB.X4DB.Manager;

namespace X4_ComplexCalculator.DB.X4DB.Builder;

/// <summary>
/// <see cref="Ship"/> クラスのインスタンスを作成するBuilderクラス
/// </summary>
class ShipBuilder
{
    #region メンバ
    /// <summary>
    /// 艦船種別一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IShipType> _shipTypes;


    /// <summary>
    /// 艦船一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, X4_DataExporterWPF.Entity.Ship> _ships;


    /// <summary>
    /// ウェアの装備情報一覧
    /// </summary>
    private readonly WareEquipmentManager _wareEquipmentManager;


    /// <summary>
    /// 艦船のハンガー情報一覧
    /// </summary>
    private readonly ShipHangerManager _shipHangerManager;


    /// <summary>
    /// 艦船のロードアウト情報一覧
    /// </summary>
    private readonly ShipLoadoutManager _shipLoadoutManager;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    /// <param name="wareEquipmentManager">ウェアの装備情報一覧</param>
    public ShipBuilder(IDbConnection conn, WareEquipmentManager wareEquipmentManager)
    {
        _shipHangerManager = new(conn);

        _shipLoadoutManager = new(conn);

        _wareEquipmentManager = wareEquipmentManager;


        // 艦船種別を初期化
        {
            const string SQL = @"SELECT ShipTypeID, Name, Description FROM ShipType";
            _shipTypes = conn.Query<ShipType>(SQL)
                .ToDictionary(x => x.ShipTypeID, x => x as IShipType);
        }

        // 艦船情報一覧を作成
        {
            _ships = conn.Query<X4_DataExporterWPF.Entity.Ship>("SELECT * FROM Ship")
                .ToDictionary(x => x.ShipID);
        }
    }


    /// <summary>
    /// 艦船情報作成
    /// </summary>
    /// <param name="ware">ベースとなるウェア情報</param>
    /// <returns>艦船情報</returns>
    public IWare Build(IWare ware)
    {
        if (!ware.Tags.Contains("ship"))
        {
            throw new ArgumentException("Ware is not Ship", nameof(ware));
        }

        if (!_ships.TryGetValue(ware.ID, out var item))
        {
            return ware;
        }

        return new Ship(
            ware,
            _shipTypes[item.ShipTypeID],
            item.Macro,
            X4Database.Instance.X4Size.Get(item.SizeID),
            item.Mass,
            new Drag(item.DragForward, item.DragReverse, item.DragHorizontal, item.DragVertical, item.DragPitch, item.DragYaw, item.DragRoll),
            new Inertia(item.InertiaPitch, item.InertiaYaw, item.InertiaRoll),
            item.Hull,
            item.People,
            item.MissileStorage,
            item.DroneStorage,
            item.CargoSize,
            _shipHangerManager.Get(ware.ID),
            _shipLoadoutManager.Get(ware.ID),
            _wareEquipmentManager.Get(ware.ID).ToDictionary(x => x.ConnectionName)
        );
    }
}
