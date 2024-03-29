﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IShipLoadout"/> の一覧を管理するクラス
/// </summary>
class ShipLoadoutManager
{
    #region メンバ
    /// <summary>
    /// 艦船のロードアウト情報一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IReadOnlyList<IShipLoadout>> _shipLoadouts;


    /// <summary>
    /// ダミー用のロードアウト情報
    /// </summary>
    private readonly IReadOnlyList<IShipLoadout> _emptyLoadouts = Array.Empty<IShipLoadout>();
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public ShipLoadoutManager(IDbConnection conn)
    {
        const string SQL = @"
SELECT
	ShipLoadout.ShipID,
	ShipLoadout.LoadoutID,
	ShipLoadout.GroupName,
	ShipLoadout.Count,
	Equipment.EquipmentID
	
FROM
	ShipLoadout,
	Equipment

WHERE
	ShipLoadout.MacroName = Equipment.MacroName";
        _shipLoadouts = conn.Query<ShipLoadout>(SQL)
            .GroupBy(x => x.ID)
            .ToDictionary(x => x.Key, x => x.ToArray() as IReadOnlyList<IShipLoadout>);
    }


    /// <summary>
    /// 艦船IDに対応するロードアウト情報一覧を取得する
    /// </summary>
    /// <param name="id">艦船ID</param>
    /// <returns>艦船IDに対応するロードアウト情報一覧</returns>
    public IReadOnlyDictionary<string, IReadOnlyList<IShipLoadout>> Get(string id)
    {
        return (_shipLoadouts.TryGetValue(id, out var loadouts) ? loadouts : _emptyLoadouts)
                .GroupBy(x => x.LoadoutID)
                .ToDictionary(x => x.Key, x => x.ToArray() as IReadOnlyList<IShipLoadout>);
    }
}
