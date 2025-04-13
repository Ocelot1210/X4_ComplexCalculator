using Collections.Pooled;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using ZLinq;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IWareEquipment"/> の一覧を管理するクラス
/// </summary>
sealed class WareEquipmentManager : IDisposable
{
    #region メンバ
    /// <summary>
    /// 空のウェアの装備情報一覧(ダミー用)
    /// </summary>
    private readonly IReadOnlyDictionary<string, IWareEquipment> _emptyEquipments = new Dictionary<string, IWareEquipment>();


    /// <summary>
    /// ウェアの装備情報一覧
    /// </summary>
    private readonly PooledDictionary<string, IReadOnlyDictionary<string, IWareEquipment>> _wareEquipments;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public WareEquipmentManager(IDbConnection conn)
    {
        // 装備一覧を作成する
        const string SQL = @"
WITH
	TmpTags AS (
		SELECT   WareID, ConnectionName, Tag
		FROM     WareEquipmentTag
		ORDER BY WareID, ConnectionName, Tag
	)

SELECT   W.WareID, W.ConnectionName, W.EquipmentTypeID, W.GroupName, group_concat(T.Tag, '彁') AS Tags
FROM     WareEquipment W, TmpTags T
WHERE    W.WareID = T.WareID AND W.ConnectionName = T.ConnectionName
GROUP BY W.WareID, W.ConnectionName
";

        int capacity = conn.QuerySingle<int>("SELECT count(*) FROM (SELECT DISTINCT WareID, ConnectionName FROM WareEquipmentTag)");
        using var tagsMgr = new TagsManager<HashSet<string>>(capacity, static x => [.. x.Split('彁')]);

        var groups = conn.Query<(string WareID, string ConnectionName, string EquipmentTypeID, string GroupName, string Tags)>(SQL)
            .AsValueEnumerable()
            .Select(x => new WareEquipment(x.WareID, x.ConnectionName, x.EquipmentTypeID, x.GroupName, tagsMgr[x.Tags]) as IWareEquipment)
            .GroupBy(x => x.ID);

        _wareEquipments = new(capacity);
        foreach (var group in groups)
        {
            _wareEquipments.Add(group.Key, group.ToDictionary(x => x.ConnectionName));
        }
    }


    /// <summary>
    /// ウェアIDをキーに、ウェアIDに対応する接続名をキーにした装備情報一覧を取得する
    /// </summary>
    /// <param name="id">ウェアID</param>
    /// <returns>ウェアIDに対応する接続名をキーにした装備情報一覧</returns>
    public IReadOnlyDictionary<string, IWareEquipment> Get(string id) => _wareEquipments.TryGetValue(id, out var value) ? value : _emptyEquipments;


    /// <inheritdoc/>
    public void Dispose()
    {
        _wareEquipments.Dispose();
    }
}
