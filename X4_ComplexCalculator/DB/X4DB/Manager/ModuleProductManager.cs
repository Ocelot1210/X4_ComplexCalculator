using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IX4Module"/> に対応する <see cref="IModuleProduct"/> の一覧を管理するクラス
/// </summary>
class ModuleProductManager
{
    #region メンバ
    /// <summary>
    /// モジュールの製品情報一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IReadOnlyList<IModuleProduct>> _ModuleProducts;


    /// <summary>
    /// ダミーの製品情報一覧
    /// </summary>
    private readonly IReadOnlyList<IModuleProduct> _DummyProduct = Array.Empty<IModuleProduct>();
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    /// <param name="wareProductionManager">ウェアの生産量と生産時間一覧</param>
    public ModuleProductManager(IDbConnection conn, WareProductionManager wareProductionManager)
    {
        const string sql = @"SELECT ModuleID, WareID, Method, Amount FROM ModuleProduct";

        _ModuleProducts = conn.Query<X4_DataExporterWPF.Entity.ModuleProduct>(sql)
            .Select(x => new ModuleProduct(x.ModuleID, x.WareID, x.Method, x.Amount, wareProductionManager.Get(x.WareID, x.Method)))
            .GroupBy(x => x.ModuleID)
            .ToDictionary(x => x.Key, x => x.ToArray() as IReadOnlyList<IModuleProduct>);
    }


    /// <summary>
    /// モジュールIDに対応するモジュールの製品情報一覧を取得する
    /// </summary>
    /// <param name="id">モジュールID</param>
    /// <returns>モジュールIDに対応するモジュールの製品情報一覧</returns>
    public IReadOnlyList<IModuleProduct> Get(string id) =>
        _ModuleProducts.TryGetValue(id, out var product) ? product : _DummyProduct;
}
