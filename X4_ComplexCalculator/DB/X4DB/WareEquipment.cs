using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// ウェアの装備情報
    /// </summary>
    public class WareEquipment
    {
        #region スタティックメンバ
        /// <summary>
        /// ダミー用ウェアの装備情報一覧
        /// </summary>
        private static readonly IReadOnlyList<WareEquipment> _DummyEquipments = Array.Empty<WareEquipment>();


        /// <summary>
        /// ウェアの装備情報一覧
        /// </summary>
        private static readonly Dictionary<string, IReadOnlyList<WareEquipment>> _WareEquipments = new();


        /// <summary>
        /// タグ一覧
        /// </summary>

        private static readonly Dictionary<string, HashSet<string>> _TagsDict = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// 識別ID
        /// </summary>
        public string ID { get; }


        /// <summary>
        /// コネクション名
        /// </summary>
        public string ConnectionName { get; }


        /// <summary>
        /// 装備種別
        /// </summary>
        public EquipmentType EquipmentType { get; }


        /// <summary>
        /// グループ名
        /// </summary>
        public string? GroupName { get; }


        /// <summary>
        /// タグ情報
        /// </summary>
        public HashSet<string> Tags { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="connectionName">コネクション名</param>
        /// <param name="equipmentTypeID">装備種別ID</param>
        /// <param name="groupName">グループ名</param>
        /// <param name="tags">タグを区切り文字で連結した文字列</param>
        private WareEquipment(string wareID, string connectionName, string equipmentTypeID, string? groupName, string tags)
        {
            ID = wareID;
            ConnectionName = connectionName;
            EquipmentType = EquipmentType.Get(equipmentTypeID);
            GroupName = groupName;
            Tags = _TagsDict[tags];
        }



        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _WareEquipments.Clear();
            InitTagsDict();

            // 単純に装備情報一覧を作成しようとすると時間がかかるため
            // Tagsを何度も作成しないように工夫する
            const string sql = @"
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
            foreach (var item in X4Database.Instance.Query<WareEquipment>(sql).GroupBy(x => x.ID))
            {
                _WareEquipments.Add(item.Key, item.ToArray());
            }
        }


        /// <summary>
        /// タグ一覧のディクショナリを初期化する
        /// </summary>
        private static void InitTagsDict()
        {
            _TagsDict.Clear();

            // Tagのユニークな組み合わせ一覧を取得する
            const string sql = @"
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

            foreach (var tagsText in X4Database.Instance.Query<string>(sql))
            {
                _TagsDict.Add(tagsText, new HashSet<string>(tagsText.Split('彁')));
            }
        }



        /// <summary>
        /// ウェアの装備情報を取得する
        /// </summary>
        /// <param name="id">ウェアID</param>
        /// <returns>ウェアIDに対応するウェアの装備情報一覧</returns>
        public static IReadOnlyList<WareEquipment> Get(string id) => _WareEquipments.TryGetValue(id, out var value) ? value : _DummyEquipments;



        /// <summary>
        /// 指定した装備がthisに装備可能か判定する
        /// </summary>
        /// <param name="equipment">判定したい装備</param>
        /// <returns>指定した装備がthisに装備可能か</returns>
        public bool CanEquipped(Equipment equipment)
            => EquipmentType == equipment.EquipmentType && !equipment.EquipmentTags.Except(Tags).Any();
    }
}
