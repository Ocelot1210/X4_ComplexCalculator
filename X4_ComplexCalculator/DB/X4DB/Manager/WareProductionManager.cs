using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IWare"/> に対応する <see cref="IWareProduction"/> の一覧を管理するクラス
/// </summary>
class WareProductionManager
{
    #region メンバ
    /// <summary>
    /// ウェアの生産量と生産時間情報の一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IWareProduction>> _wareProductions;


    /// <summary>
    /// ダミー用ウェア生産情報
    /// </summary>
    private readonly IReadOnlyDictionary<string, IWareProduction> _dummyWareProduction = new Dictionary<string, IWareProduction>();
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public WareProductionManager(IDbConnection conn)
    {
        const string SQL = "SELECT WareID, Method, Name, Amount, Time FROM WareProduction";

        _wareProductions = conn.Query<WareProduction>(SQL)
            .GroupBy(x => x.WareID)
            .ToDictionary(
                x => x.Key,
                x => x.ToDictionary(y => y.Method, y => y as IWareProduction) as IReadOnlyDictionary<string, IWareProduction>
            );
    }



    /// <summary>
    /// ウェアIDに対応するウェア生産情報一覧を取得
    /// </summary>
    /// <param name="id">ウェアID</param>
    /// <returns>ウェアIDに対応するウェア生産方式一覧</returns>
    public IReadOnlyDictionary<string, IWareProduction> Get(string id)
        => _wareProductions.TryGetValue(id, out var ret) ? ret : _dummyWareProduction;



    /// <summary>
    /// ウェアIDと生産方式に対応する生産情報を取得する
    /// </summary>
    /// <param name="id">ウェアID</param>
    /// <param name="method">生産方式</param>
    /// <returns>ウェアIDと生産方式に対応する生産情報</returns>
    public IWareProduction Get(string id, string method)
    {
        var productions = Get(id);

        if (productions.TryGetValue(method, out var production))
        {
            return production;
        }

        return productions["default"];
    }
}
