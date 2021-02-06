﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    class WareOwnerManager
    {
        #region メンバ
        /// <summary>
        /// ユニークなウェア所有派閥一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, IReadOnlyList<Faction>> _Owners;


        /// <summary>
        /// ウェアIDとタグ文字列のペア
        /// </summary>
        private readonly IReadOnlyDictionary<string, string> _WareOwnerPair;


        /// <summary>
        /// 空の所有派閥一覧(ダミー用)
        /// </summary>
        private readonly IReadOnlyList<Faction> _EmptyOwners = Array.Empty<Faction>();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public WareOwnerManager(IDbConnection conn)
        {
            // ユニークなウェア所有派閥一覧を作成
            {
                const string sql = @"
SELECT   DISTINCT group_concat(TmpOwner.FactionID, '彁') As Tags
FROM     (SELECT WareOwner.WareID, WareOwner.FactionID FROM WareOwner ORDER BY WareOwner.WareID, WareOwner.FactionID) TmpOwner
GROUP BY TmpOwner.WareID";

                _Owners = conn.Query<string>(sql)
                    .ToDictionary(
                        x => x,
                        x => x.Split('彁')
                            .Select(y => X4Database.Instance.Faction.Get(y))
                            .Where(y => y is not null)
                            .Select(y => y!)
                            .ToArray() as IReadOnlyList<Faction>
                    );
            }


            // ウェアIDとウェア所有派閥一覧文字列のペアを作成する
            {
                const string sql = @"
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

                _WareOwnerPair = conn.Query<(string WareID, string Tags)>(sql)
                    .ToDictionary(x => x.WareID, x => x.Tags);
            }
        }


        /// <summary>
        /// ウェアIDに対応する所有派閥一覧を取得する
        /// </summary>
        /// <param name="wareID"></param>
        /// <returns></returns>
        public IReadOnlyList<Faction> Get(string wareID)
        {
            if (_WareOwnerPair.TryGetValue(wareID, out var owners))
            {
                if (_Owners.TryGetValue(owners, out var factions))
                {
                    return factions;
                }
            }

            return _EmptyOwners;
        }
    }
}
