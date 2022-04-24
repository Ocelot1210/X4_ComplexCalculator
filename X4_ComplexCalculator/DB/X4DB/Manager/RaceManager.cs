using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IRace"/> の一覧を管理するクラス
/// </summary>
public class RaceManager
{
    #region メンバ
    /// <summary>
    /// 種族IDをキーにした <see cref="IRace"/> の一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IRace> _Races;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public RaceManager(IDbConnection conn)
    {
        const string sql = "SELECT RaceID, Name, ShortName, Description FROM Race";
        _Races = conn.Query<Race>(sql).ToDictionary(x => x.RaceID, x => x as IRace);
    }


    /// <summary>
    /// <paramref name="id"/> に対応する <see cref="IRace"/> の取得を試みる
    /// </summary>
    /// <param name="id">種族ID</param>
    /// <returns>
    /// <para><paramref name="id"/> に対応する <see cref="IRace"/></para>
    /// <para><paramref name="id"/> に対応する <see cref="IRace"/> が無ければ null</para></returns>
    public IRace? TryGet(string id) =>
        _Races.TryGetValue(id, out var race) ? race : null;


    /// <summary>
    /// <paramref name="id"/> に対応する <see cref="IRace"/> を取得する
    /// </summary>
    /// <param name="id">種族ID</param>
    /// <returns><paramref name="id"/> に対応する <see cref="IRace"/></returns>
    /// <exception cref="KeyNotFoundException"><paramref name="id"/> に対応する <see cref="IRace"/> が無い場合</exception>
    public IRace Get(string id) => _Races[id];


    /// <summary>
    /// 全ての <see cref="IRace"/> を取得する
    /// </summary>
    /// <returns>全ての <see cref="IRace"/> の列挙</returns>
    public IEnumerable<IRace> GetAll() => _Races.Values;
}
