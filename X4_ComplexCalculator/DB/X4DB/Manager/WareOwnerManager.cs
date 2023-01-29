using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IWare.Owners"/> のユニークな組み合わせを管理するクラス
/// </summary>
class WareOwnerManager
{
    #region メンバ
    /// <summary>
    /// ユニークなウェア所有派閥一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IReadOnlyList<IFaction>> _owners;


    /// <summary>
    /// ウェアIDとタグ文字列のペア
    /// </summary>
    private readonly IReadOnlyDictionary<string, string> _wareOwnerPair;


    /// <summary>
    /// 空の所有派閥一覧(ダミー用)
    /// </summary>
    private readonly IReadOnlyList<IFaction> _emptyOwners = Array.Empty<IFaction>();
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public WareOwnerManager(IDbConnection conn)
    {
        // ユニークなウェア所有派閥一覧を作成
        {
            const string SQL = @"
SELECT   DISTINCT group_concat(TmpOwner.FactionID, '彁') As Tags
FROM     (SELECT WareOwner.WareID, WareOwner.FactionID FROM WareOwner ORDER BY WareOwner.WareID, WareOwner.FactionID) TmpOwner
GROUP BY TmpOwner.WareID";

            _owners = conn.Query<string>(SQL)
                .ToDictionary(
                    x => x,
                    x => x.Split('彁')
                        .Select(y => X4Database.Instance.Faction.TryGet(y))
                        .Where(y => y is not null)
                        .Select(y => y!)
                        .ToArray() as IReadOnlyList<IFaction>
                );
        }


        // ウェアIDとウェア所有派閥一覧文字列のペアを作成する
        {
            const string SQL = @"
SELECT
	Ware.WareID,
	group_concat(TmpOwner.FactionID, '彁') As Tags
	
FROM
	Ware,
	(SELECT WareOwner.WareID, WareOwner.FactionID FROM WareOwner ORDER BY WareOwner.WareID, WareOwner.FactionID) TmpOwner

WHERE
	Ware.WareID = TmpOwner.WareID
	
GROUP BY
	Ware.WareID";

            _wareOwnerPair = conn.Query<(string WareID, string Tags)>(SQL)
                .ToDictionary(x => x.WareID, x => x.Tags);
        }
    }


    /// <summary>
    /// <see cref="IWare.ID"/>に対応するウェアの所有派閥一覧を取得する
    /// </summary>
    /// <param name="wareID"><see cref="IWare.ID"/></param>
    /// <returns><see cref="IWare.ID"/>に対応するウェアの所有派閥一覧</returns>
    public IReadOnlyList<IFaction> Get(string wareID)
    {
        if (_wareOwnerPair.TryGetValue(wareID, out var owners))
        {
            if (_owners.TryGetValue(owners, out var factions))
            {
                return factions;
            }
        }

        return _emptyOwners;
    }
}
