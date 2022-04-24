using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IFaction"/> の一覧を管理するクラス
/// </summary>
class FactionManager
{
    #region メンバ
    /// <summary>
    /// 派閥一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IFaction> _Factions;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    /// <param name="raceManager">種族情報</param>
    public FactionManager(IDbConnection conn, RaceManager raceManager)
    {
        _Factions = conn.Query<X4_DataExporterWPF.Entity.Faction>("SELECT * FROM Faction WHERE RaceID IN (SELECT RaceID FROM Race)")
            .Select(x => new Faction(x.FactionID, x.Name, raceManager.Get(x.RaceID)) as IFaction)
            .ToDictionary(x => x.FactionID);
    }


    /// <summary>
    /// 派閥IDに対応する派閥の取得を試みる
    /// </summary>
    /// <param name="id">種族ID</param>
    /// <returns>派閥IDに対応する派閥 派閥IDに対応する派閥が無ければnull</returns>
    public IFaction? TryGet(string id) =>
        _Factions.TryGetValue(id, out var race) ? race : null;
}
