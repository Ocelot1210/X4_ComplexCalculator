using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IShip"/> に対応する <see cref="IShipHanger"/> の一覧を管理するクラス
/// </summary>
class ShipHangerManager
{
    #region メンバ
    /// <summary>
    /// 艦船のハンガー一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IShipHanger>> _shipHangers;


    /// <summary>
    /// 空のハンガー情報(ダミー用)
    /// </summary>
    private readonly IReadOnlyDictionary<string, IShipHanger> _emptyHanger = new Dictionary<string, IShipHanger>();
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public ShipHangerManager(IDbConnection conn)
    {
        const string SQL = @"SELECT ShipID, SizeID, Count, Capacity FROM ShipHanger";
        _shipHangers = conn.Query<ShipHanger>(SQL)
            .GroupBy(x => x.ShipID)
            .ToDictionary(
                x => x.Key,
                x => x.ToDictionary(y => y.Size.SizeID, y => y as IShipHanger) as IReadOnlyDictionary<string, IShipHanger>);
    }


    /// <summary>
    /// 艦船IDに対応するハンガー情報を取得する
    /// </summary>
    /// <param name="id">艦船ID</param>
    /// <returns>
    /// <para><paramref name="id"/> に対応する <see cref="IX4Size.SizeID"/> をキーにしたハンガー情報のディクショナリ</para>
    /// <para>ハンガー情報が無い場合、空の <see cref="IX4Size.SizeID"/> をキーにしたハンガー情報のディクショナリ</para>
    /// </returns>
    public IReadOnlyDictionary<string, IShipHanger> Get(string id) =>
        _shipHangers.TryGetValue(id, out var hanger) ? hanger : _emptyHanger;
}
