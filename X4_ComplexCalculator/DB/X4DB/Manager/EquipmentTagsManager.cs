using Collections.Pooled;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using ZLinq;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IEquipment.EquipmentTags"/> のユニークな組み合わせを管理するクラス
/// </summary>
sealed class EquipmentTagsManager : IDisposable
{
    #region メンバ
    /// <summary>
    /// 装備IDとタグのペア
    /// </summary>
    private readonly PooledDictionary<string, HashSet<string>> _equipmentTagsPair;


    /// <summary>
    /// 装備IDとサイズのペア
    /// </summary>
    private readonly PooledDictionary<string, IX4Size?> _equipmentSizePair;


    /// <summary>
    /// ダミー用のタグ一覧
    /// </summary>
    private readonly HashSet<string> _dummyTags = [];
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public EquipmentTagsManager(IDbConnection conn)
    {
        // 装備IDとタグ文字列一覧のペアを作成する
        {
            const string SQL1 = @"
WITH
	TmpTags AS (
		SELECT	 E.EquipmentID, E.Tag
		FROM 	 EquipmentTag E
		ORDER BY E.EquipmentID, E.Tag
	)

SELECT   E.EquipmentID, group_concat(T.Tag, '彁') AS Tags
FROM 	 Equipment E, TmpTags T
WHERE    E.EquipmentID = T.EquipmentID
GROUP BY E.EquipmentID
";
            int capacity = conn.QuerySingle<int>("SELECT count(*) FROM (SELECT DISTINCT EquipmentID FROM EquipmentTag)");
            using var tagsMgr = new TagsManager<HashSet<string>>(capacity, static x => [.. x.Split('彁')]);

            _equipmentTagsPair = new(capacity);
            foreach (var (wareID, tags) in conn.Query<(string, string)>(SQL1))
            {
                _equipmentTagsPair.Add(wareID, tagsMgr[tags]);
            }
        }
        
        // 装備IDに対応するサイズIDのペアを作成する
        {
            const string SQL2 = @"
WITH
	EqpSize AS (
		SELECT DISTINCT T.EquipmentID, S.SizeID
		FROM   EquipmentTag T, Size S
		WHERE  T.Tag = S.SizeID
	)

SELECT E.EquipmentID, S.SizeID
FROM   Equipment E, EqpSize S
WHERE  E.EquipmentID = S.EquipmentID
";

            _equipmentSizePair = conn.Query<(string EquipmentID, string SizeID)>(SQL2)
                .ToPooledDictionary(x => x.EquipmentID, x => X4Database.Instance.X4Size.TryGet(x.SizeID));
        }
    }


    /// <summary>
    /// 装備IDに対応するタグ一覧を取得する
    /// </summary>
    /// <param name="id">装備ID</param>
    /// <returns>装備IDに対応するタグ一覧</returns>
    public HashSet<string> GetTags(string id) => _equipmentTagsPair.TryGetValue(id, out var tags) ? tags : _dummyTags;


    /// <summary>
    /// 装備IDに対応するサイズの取得を試みる
    /// </summary>
    /// <param name="id">装備ID</param>
    /// <returns>装備IDに対応するサイズ</returns>
    public IX4Size? TryGetSize(string id) => _equipmentSizePair.TryGetValue(id, out var size) ? size : null;


    /// <inheritdoc/>
    public void Dispose()
    {
        _equipmentTagsPair.Dispose();
        _equipmentSizePair.Dispose();
    }
}
