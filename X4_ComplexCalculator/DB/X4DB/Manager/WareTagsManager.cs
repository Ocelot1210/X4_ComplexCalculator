using Collections.Pooled;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

sealed class WareTagsManager : IDisposable
{
    #region メンバ
    /// <summary>
    /// ウェアIDとタグ文字列のペア
    /// </summary>
    private readonly PooledDictionary<string, HashSet<string>> _wareTagsPair;


    /// <summary>
    /// 空のタグ一覧(ダミー用)
    /// </summary>
    private readonly HashSet<string> _emptyTags = new();
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public WareTagsManager(IDbConnection conn)
    {
        // ウェアIDとタグ一覧を作成する
        const string SQL = @"
WITH
	TmpTags AS (
		SELECT	 DISTINCT W.WareID, W.Tag
		FROM 	 WareTags W
		ORDER BY W.WareID, W.Tag
	)

SELECT   W.WareID, group_concat(T.Tag, '彁') AS Tags
FROM     Ware W, TmpTags T
WHERE    W.WareID = T.WareID AND W.TransportTypeID <> 'inventory'
GROUP BY W.WareID
";
        int capacity = conn.QuerySingle<int>("SELECT count(*) FROM Ware WHERE TransportTypeID <> 'inventory'");
        _wareTagsPair = new(capacity);

        using var tagsMgr = new TagsManager<HashSet<string>>(capacity, static x => [.. x.Split('彁')]);

        foreach (var (wareID, tags) in conn.Query<(string, string)>(SQL))
        {
            _wareTagsPair.Add(wareID, tagsMgr[tags]);
        }        
    }


    /// <summary>
    /// ウェアIDに対応するタグ一覧を取得する
    /// </summary>
    /// <param name="wareID">ウェアID</param>
    /// <returns>タグ一覧</returns>
    public HashSet<string> Get(string wareID) => _wareTagsPair.TryGetValue(wareID, out var tags) ? tags : _emptyTags;


    /// <inheritdoc/>
    public void Dispose()
    {
        _wareTagsPair.Dispose();
    }
}
