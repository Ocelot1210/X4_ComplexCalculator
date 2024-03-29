﻿using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IWareResource"/> の一覧を管理する
/// </summary>
class WareResourceManager
{
    #region メンバ
    /// <summary>
    /// 1サイクルのウェア生産に必要なウェア情報一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<IWareResource>>> _wareResources;


    /// <summary>
    /// ダミー用1サイクルのウェア生産に必要なウェア情報一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IReadOnlyList<IWareResource>> _dummyResources
        = new Dictionary<string, IReadOnlyList<IWareResource>>();
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public WareResourceManager(IDbConnection conn)
    {
        // ウェア生産情報を作成する
        {
            const string SQL = "SELECT WareID, Method, NeedWareID, Amount FROM WareResource";

            _wareResources = conn.Query<WareResource>(SQL)
                .GroupBy(x => x.WareID)
                .ToDictionary(
                    x => x.Key,
                    x => x.GroupBy(y => y.Method).ToDictionary(y => y.Key, y => y.ToArray() as IReadOnlyList<IWareResource>) as IReadOnlyDictionary<string, IReadOnlyList<IWareResource>>
                );
        }
    }


    /// <summary>
    /// ウェアIDに対応するウェア生産に必要なウェア情報一覧を取得する
    /// </summary>
    /// <param name="wareID">ウェアID</param>
    /// <returns>ウェアIDに対応するウェア生産に必要なウェア情報一覧</returns>
    public IReadOnlyDictionary<string, IReadOnlyList<IWareResource>> Get(string wareID)
        => _wareResources.TryGetValue(wareID, out var resources) ? resources : _dummyResources;
}
