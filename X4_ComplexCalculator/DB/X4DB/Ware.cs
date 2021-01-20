using System;
using System.Collections.Generic;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// ウェア情報管理用クラス
    /// </summary>
    public class Ware
    {
        #region スタティックメンバ
        /// <summary>
        /// ウェア一覧
        /// </summary>
        protected readonly static Dictionary<string, Ware> _Wares = new();


        /// <summary>
        /// タグ一覧
        /// </summary>

        private static readonly Dictionary<string, HashSet<string>> _TagsDict = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// ウェアID
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// ウェア名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// ウェア種別
        /// </summary>
        public WareGroup WareGroup { get; }


        /// <summary>
        /// カーゴ種別
        /// </summary>
        public TransportType TransportType { get; }


        /// <summary>
        /// 説明文
        /// </summary>
        public string Description { get; }


        /// <summary>
        /// コンテナサイズ
        /// </summary>
        public long Volume { get; }


        /// <summary>
        /// 最低価格
        /// </summary>
        public long MinPrice { get; }


        /// <summary>
        /// 平均値
        /// </summary>
        public long AvgPrice { get; }


        /// <summary>
        /// 最高価格
        /// </summary>
        public long MaxPrice { get; }


        /// <summary>
        /// 所有派閥
        /// </summary>
        public IReadOnlyList<Faction> Owners { get; }


        /// <summary>
        /// 生産方式
        /// </summary>
        public IReadOnlyList<WareProduction> Productions { get; }


        /// <summary>
        /// 装備一覧
        /// </summary>
        public IReadOnlyDictionary<string, WareEquipment> Equipments { get; }


        /// <summary>
        /// 生産に必要なウェア情報
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<WareResource>> Resources { get; }


        /// <summary>
        /// タグ一覧
        /// </summary>
        public HashSet<string> Tags { get; }
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="tags">タグ一覧</param>
        protected Ware(string wareID, string tags)
        {
            ID = wareID;

            var keyObj = new { WareID = wareID };
            const string sql1 = @"
SELECT
    WareGroupID, TransportTypeID, Name, Description, Volume, MinPrice, AvgPrice, MaxPrice
FROM
    Ware
WHERE
    WareID = :WareID";
            string? wareGroupID;
            string transportTypeID;

            (
                wareGroupID,
                transportTypeID,
                Name,
                Description,
                Volume,
                MinPrice,
                AvgPrice,
                MaxPrice
            ) = X4Database.Instance.QuerySingle<(string?, string, string, string, long, long, long, long)>(sql1, keyObj);

            WareGroup = WareGroup.Get(wareGroupID ?? "");

            TransportType = TransportType.Get(transportTypeID);

            const string sql2 = @"SELECT FactionID FROM WareOwner WHERE WareID = :WareID";
            Owners = X4Database.Instance.Query<string>(sql2, keyObj)
                .Select(x => Faction.Get(x))
                .Where(x => x is not null)
                .Select(x => x!)
                .ToArray();

            Productions = WareProduction.Get(wareID).Values.ToArray();

            Equipments = WareEquipment.Get(wareID)
                .ToDictionary(x => x.ConnectionName);

            Resources = WareResource.Get(wareID);
            Tags = _TagsDict[tags];
        }


        /// <summary>
        /// ウェアを作成する
        /// </summary>
        /// <param name="id">ウェアID</param>
        /// <param name="tagsText">タグ一覧</param>
        /// <returns>ウェアIDに対応するウェア</returns>
        private static Ware Create(string id, string tagsText, HashSet<string> tags)
        {
            // ステーションモジュールの場合
            if (tags.Contains("module"))
            {
                return new Module(id, tagsText);
            }
            
            // 装備の場合
            if (tags.Contains("equipment"))
            {
                return Equipment.Create(id, tagsText) ?? new Ware(id, tagsText);
            }

            // 艦船の場合
            if (tags.Contains("ship"))
            {
                return new Ship(id, tagsText);
            }

            // それ以外の場合
            return new Ware(id, tagsText);
        }



        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _Wares.Clear();

            WareProduction.Init();
            WareEquipment.Init();
            WareEffect.Init();
            WareResource.Init();

            ModuleType.Init();
            ModuleProduct.Init();
            ModuleStorage.Init();
            ShipHanger.Init();
            ShipType.Init();

            InitTagsDict();
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
            foreach (var (wareID, tags) in X4Database.Instance.Query<(string wareID, string tags)>(sql))
            {
                _Wares.Add(wareID, Create(wareID, tags, _TagsDict[tags]));
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
			WareTags.WareID,
			WareTags.Tag
		FROM
			WareTags
		ORDER BY
			WareTags.WareID,
			WareTags.Tag
	) TmpTagsTable

GROUP BY
	TmpTagsTable.WareID";

            foreach (var tagsText in X4Database.Instance.Query<string>(sql))
            {
                _TagsDict.Add(tagsText, new HashSet<string>(tagsText.Split('彁')));
            }
        }








        /// <summary>
        /// ウェアIDに対応するウェアを取得する
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <returns>ウェア</returns>
        public static Ware Get(string wareID) => _Wares[wareID];


        /// <summary>
        /// ウェアIDに対応するウェアを取得する(型指定版)
        /// </summary>
        /// <typeparam name="T">指定した型</typeparam>
        /// <param name="wareID">ウェアID</param>
        /// <returns>指定した型のウェア</returns>
        public static T Get<T>(string wareID) where T : Ware => (T)_Wares[wareID];



        /// <summary>
        /// ウェアIDに対応するウェアの取得を試みる(型指定版)
        /// </summary>
        /// <typeparam name="T">指定した型</typeparam>
        /// <param name="id">ウェアID</param>
        /// <returns>指定した型のウェア又はnull</returns>
        public static T? TryGet<T>(string id) where T : Ware => _Wares.TryGetValue(id, out var ret) ? ret as T : null;



        /// <summary>
        /// 全ウェアを取得
        /// </summary>
        public static IEnumerable<Ware> GetAll() => _Wares.Values;


        /// <summary>
        /// 全ウェアを取得(型指定版)
        /// </summary>
        /// <typeparam name="T">指定した型</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetAll<T>() where T : Ware => _Wares.Values.OfType<T>();


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is Ware tgt && tgt.ID == ID;


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool Equals(Ware other) => ID == other.ID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(ID);


        /// <summary>
        /// 指定したコネクション名に装備可能なEquipmentを取得する
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<T> GetEquippableEquipment<T>(string connectionName) where T : Equipment
        {
            if (Equipments.TryGetValue(connectionName, out var wareEquipment))
            {
                return _Wares.Values
                    .OfType<T>()
                    .Where(x => !x.EquipmentTags.Except(wareEquipment.Tags).Any());
            }

            return Enumerable.Empty<T>();
        }
    }
}
