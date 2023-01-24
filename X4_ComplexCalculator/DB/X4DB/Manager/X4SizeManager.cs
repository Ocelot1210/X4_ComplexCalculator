using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IX4Size"/> の一覧を管理するクラス
/// </summary>
class X4SizeManager
{
    #region メンバ
    /// <summary>
    /// サイズIDをキーにした <see cref="IX4Size"/> の一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IX4Size> _Sizes;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public X4SizeManager(IDbConnection conn)
    {
        const string sql = "SELECT SizeID, Name FROM Size";
        _Sizes = conn.Query<X4Size>(sql)
            .ToDictionary(x => x.SizeID, x => x as IX4Size);
    }


    /// <summary>
    /// <paramref name="id"/> に対応する <see cref="IX4Size"/> を取得
    /// </summary>
    /// <param name="id">サイズID</param>
    /// <returns><paramref name="id"/>に対応する <see cref="IX4Size"/></returns>
    /// <exception cref="KeyNotFoundException"><paramref name="id"/>に対応する <see cref="IX4Size"/> が無い場合</exception>
    public IX4Size Get(string id) => _Sizes[id];



    /// <summary>
    /// <paramref name="id"/> に対応する <see cref="IX4Size"/> の取得を試みる
    /// </summary>
    /// <param name="id">サイズID</param>
    /// <returns>
    /// <para><paramref name="id"/> に対応する <see cref="IX4Size"/></para>
    /// <para><paramref name="id"/> に対応するサイズが無ければnull</para>
    /// </returns>
    public IX4Size? TryGet(string id) => _Sizes.TryGetValue(id, out var ret) ? ret : null;
}
