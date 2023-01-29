using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IWareEquipment"/> の一覧を管理するクラス
/// </summary>
class WareEquipmentManager
{
    #region メンバ
    /// <summary>
    /// 空のウェアの装備情報一覧(ダミー用)
    /// </summary>
    private readonly IReadOnlyList<IWareEquipment> _emptyEquipments = Array.Empty<IWareEquipment>();


    /// <summary>
    /// ウェアの装備情報一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IReadOnlyList<IWareEquipment>> _wareEquipments;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public WareEquipmentManager(IDbConnection conn)
    {
        // Tagのユニークな組み合わせ一覧を作成する
        const string SQL_1 = @"
SELECT
	DISTINCT group_concat(TmpTagsTable.Tag, '彁') As Tags
	
FROM
	(
		SELECT
			WareEquipmentTag.WareID,
			WareEquipmentTag.ConnectionName,
			WareEquipmentTag.Tag
		FROM
			WareEquipmentTag
		ORDER BY
			WareEquipmentTag.WareID,
			WareEquipmentTag.ConnectionName,
			WareEquipmentTag.Tag
	) TmpTagsTable

GROUP BY
	TmpTagsTable.WareID,
	TmpTagsTable.ConnectionName";

        var tagsDict = conn.Query<string>(SQL_1)
            .ToDictionary(x => x, x => new HashSet<string>(x.Split('彁')));
        


        // 装備一覧を作成する
        const string SQL_2 = @"
SELECT
	WareEquipment.WareID,
	WareEquipment.ConnectionName,
	WareEquipment.EquipmentTypeID,
	WareEquipment.GroupName,
	group_concat(Sorted_WareEquipmentTag.Tag, '彁') AS Tags
	
FROM
	WareEquipment,
	(SELECT * FROM WareEquipmentTag ORDER BY WareEquipmentTag.WareID, WareEquipmentTag.ConnectionName, WareEquipmentTag.Tag) Sorted_WareEquipmentTag
	
WHERE
	WareEquipment.WareID = Sorted_WareEquipmentTag.WareID AND
	WareEquipment.ConnectionName = Sorted_WareEquipmentTag.ConnectionName
	
GROUP BY
	WareEquipment.WareID,
	WareEquipment.ConnectionName";

        _wareEquipments = conn.Query<TempWareEquipment>(SQL_2)
            .Select(x => new WareEquipment(x.WareID, x.ConnectionName, x.EquipmentTypeID, x.GroupName, tagsDict[x.Tags]))
            .GroupBy(x => x.ID)
            .ToDictionary(x => x.Key, x => x.ToArray() as IReadOnlyList<IWareEquipment>);
        
    }


    /// <summary>
    /// ウェアの装備情報を取得する
    /// </summary>
    /// <param name="id">ウェアID</param>
    /// <returns>ウェアIDに対応するウェアの装備情報一覧</returns>
    public IReadOnlyList<IWareEquipment> Get(string id) => _wareEquipments.TryGetValue(id, out var value) ? value : _emptyEquipments;


    /// <summary>
    /// 装備一覧作成時の一時情報用クラス
    /// </summary>
    private class TempWareEquipment
    {
        public string WareID { get; }
        public string ConnectionName { get; }
        public string EquipmentTypeID { get; }
        public string GroupName { get; }
        public string Tags { get; }

        public TempWareEquipment(
            string wareID,
            string connectionName,
            string equipmentTypeID,
            string groupName,
            string tags
        )
        {
            WareID = wareID;
            ConnectionName = connectionName;
            EquipmentTypeID = equipmentTypeID;
            GroupName = groupName;
            Tags = tags;
        }
    }
}
