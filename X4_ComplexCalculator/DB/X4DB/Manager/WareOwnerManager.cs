using Collections.Pooled;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using ZLinq;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IWare.Owners"/> のユニークな組み合わせを管理するクラス
/// </summary>
sealed class WareOwnerManager : IDisposable
{
    #region メンバ
    /// <summary>
    /// ウェアIDと所有派閥リストのペア
    /// </summary>
    private readonly PooledDictionary<string, IFaction[]> _wareOwnerPair;


    /// <summary>
    /// 空の所有派閥一覧(ダミー用)
    /// </summary>
    private readonly IReadOnlyList<IFaction> _emptyOwners = [];
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public WareOwnerManager(IDbConnection conn)
    {
        // ウェアIDとウェア所有派閥一覧文字列のペアを作成する
        {
            const string SQL = @"
With
	TmpOwners AS (
		SELECT   WareOwner.WareID, WareOwner.FactionID
		FROM     WareOwner
		ORDER BY WareOwner.WareID, WareOwner.FactionID
	)

SELECT   W.WareID, group_concat(O.FactionID, '彁') As Tags
FROM     Ware W, TmpOwners O
WHERE    W.WareID = O.WareID
GROUP BY W.WareID
";
            int capacity = conn.QuerySingle<int>("SELECT count(*) FROM (SELECT DISTINCT WareID FROM WareOwner)");
            using var ownerMgr = new TagsManager<IFaction[]>(capacity, static x => x.Split('彁').AsValueEnumerable().Select(x => X4Database.Instance.Faction.TryGet(x)).Where(x => x is not null).Select(x => x!).ToArray());

            _wareOwnerPair = new(capacity);

            foreach (var (wareID, tag) in conn.Query<(string, string)>(SQL))
            {
                _wareOwnerPair.Add(wareID, ownerMgr[tag]);
            }
        }
    }


    /// <summary>
    /// <see cref="IWare.ID"/>に対応するウェアの所有派閥一覧を取得する
    /// </summary>
    /// <param name="wareID"><see cref="IWare.ID"/></param>
    /// <returns><see cref="IWare.ID"/>に対応するウェアの所有派閥一覧</returns>
    public IReadOnlyList<IFaction> Get(string wareID) => _wareOwnerPair.TryGetValue(wareID, out var owners) ? owners : _emptyOwners;


    /// <inheritdoc/>
    public void Dispose()
    {
        _wareOwnerPair.Dispose();
    }
}
