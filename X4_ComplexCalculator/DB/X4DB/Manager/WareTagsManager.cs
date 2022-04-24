using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

class WareTagsManager
{
    #region メンバ
    /// <summary>
    /// Tagのユニークな組み合わせ一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, HashSet<string>> _Tags;


    /// <summary>
    /// ウェアIDとタグ文字列のペア
    /// </summary>
    private readonly IReadOnlyDictionary<string, string> _WareTagsPair;


    /// <summary>
    /// 空のタグ一覧(ダミー用)
    /// </summary>
    private readonly HashSet<string> _EmptyTags = new();
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public WareTagsManager(IDbConnection conn)
    {
        // Tagのユニークな組み合わせ一覧を作成する
        {
            const string sql = @"
SELECT
	DISTINCT group_concat(TmpTagsTable.Tag, '彁') As Tags
	
FROM
	(SELECT WareTags.WareID, WareTags.Tag FROM WareTags ORDER BY WareTags.WareID, WareTags.Tag) TmpTagsTable

GROUP BY
	TmpTagsTable.WareID";

            _Tags = conn.Query<string>(sql)
                .ToDictionary(x => x, x => new HashSet<string>(x.Split('彁')));
        }

        // ウェアIDとタグ文字列のペアを作成する
        {
            const string sql = @"
SELECT
	Ware.WareID,
	group_concat(Sorted_WareTags.Tag, '彁') AS Tags
FROM
	Ware,
	(SELECT WareTags.WareID, WareTags.Tag FROM WareTags ORDER BY WareTags.WareID, WareTags.Tag) Sorted_WareTags
	
WHERE
	Ware.WareID = Sorted_WareTags.WareID AND
	Ware.TransportTypeID <> 'inventory'

GROUP BY
	Ware.WareID";

            _WareTagsPair = conn.Query<(string WareID, string Tags)>(sql)
                .ToDictionary(x => x.WareID, x => x.Tags);
        }
    }



    /// <summary>
    /// ウェアIDに対応するタグ一覧を取得する
    /// </summary>
    /// <param name="wareID">ウェアID</param>
    /// <returns>タグ一覧</returns>
    public HashSet<string> Get(string wareID)
    {
        if (_WareTagsPair.TryGetValue(wareID, out var tagsText))
        {
            if (_Tags.TryGetValue(tagsText, out var tags))
            {
                return tags;
            }
        }

        return _EmptyTags;
    }
}
