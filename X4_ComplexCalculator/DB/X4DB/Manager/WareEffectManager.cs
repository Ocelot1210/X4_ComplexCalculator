using Collections.Pooled;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;


namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IWareEffect"/> の一覧を管理するクラス
/// </summary>
sealed class WareEffectManager : IDisposable
{
    #region メンバ
    /// <summary>
    /// <see cref="IWare.ID"/> をキーにしたウェア生産時の追加効果情報一覧
    /// </summary>
    private readonly PooledDictionary<string, WareEffects> _wareEffects;


    /// <summary>
    /// 空のウェア生産時の追加効果情報
    /// </summary>
    private readonly IWareEffects _emptyEffect = new WareEffects([]);
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public WareEffectManager(IDbConnection conn)
    {
        const string SQL = "SELECT WareID, Method, EffectID, Product FROM WareEffect";

        _wareEffects = conn.Query<WareEffect>(SQL)
            .GroupBy(x => x.WareID)
            .ToPooledDictionary(x => x.Key, x => new WareEffects(x));
    }


    /// <summary>
    ///<see cref="IWare.ID"/> に対応するウェア生産時の追加効果情報一覧を取得する
    /// </summary>
    /// <param name="id"><see cref="IWare.ID"/></param>
    /// <returns><paramref name="id"/> に対応するウェア生産時の追加効果情報一覧</returns>
    public IWareEffects Get(string id)
        => _wareEffects.TryGetValue(id, out var effects) ? effects : _emptyEffect;


    /// <inheritdoc/>
    public void Dispose()
    {
        _wareEffects.Dispose();
    }
}
