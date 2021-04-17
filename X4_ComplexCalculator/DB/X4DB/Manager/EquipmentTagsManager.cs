using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// <see cref="IEquipment.EquipmentTags"/> のユニークな組み合わせを管理するクラス
    /// </summary>
    class EquipmentTagsManager
    {
        #region メンバ
        /// <summary>
        /// Tagのユニークな組み合わせ一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, HashSet<string>> _Tags;


        /// <summary>
        /// ウェアIDとタグ文字列のペア
        /// </summary>
        private readonly IReadOnlyDictionary<string, string> _EquipmentTagsPair;


        /// <summary>
        /// ダミー用のタグ一覧
        /// </summary>
        private readonly HashSet<string> _DummyTags = new();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public EquipmentTagsManager(IDbConnection conn)
        {
            // Tagのユニークな組み合わせ一覧を作成する
            {
                const string sql = @"
SELECT
	DISTINCT group_concat(TmpTagsTable.Tag, '彁') As Tags
	
FROM
	(SELECT EquipmentTag.EquipmentID, EquipmentTag.Tag FROM EquipmentTag ORDER BY EquipmentTag.EquipmentID, EquipmentTag.Tag) TmpTagsTable

GROUP BY
	TmpTagsTable.EquipmentID";

                _Tags = conn.Query<string>(sql)
                    .ToDictionary(x => x, x => new HashSet<string>(x.Split('彁')));
            }


            // 装備IDとタグ文字列のペアを作成する
            {
                const string sql = @"
SELECT
	Equipment.EquipmentID ,
	group_concat(TmpTagsTable.Tag, '彁') As Tags
	
FROM
	Equipment,
	(SELECT EquipmentTag.EquipmentID, EquipmentTag.Tag FROM EquipmentTag ORDER BY EquipmentTag.EquipmentID, EquipmentTag.Tag) TmpTagsTable

WHERE
	Equipment.EquipmentID = TmpTagsTable.EquipmentID
	
GROUP BY
	Equipment.EquipmentID ";

                _EquipmentTagsPair = conn.Query<(string WareID, string Tags)>(sql)
                    .ToDictionary(x => x.WareID, x => x.Tags);
            }
        }


        /// <summary>
        /// 装備IDに対応するタグ一覧を取得する
        /// </summary>
        /// <param name="id">装備ID</param>
        /// <returns>装備IDに対応するタグ一覧</returns>
        public HashSet<string> Get(string id)
        {
            if (_EquipmentTagsPair.TryGetValue(id, out var tags))
            {
                if (_Tags.TryGetValue(tags, out var ret))
                {
                    return ret;
                }
            }

            return _DummyTags;
        }
    }
}
