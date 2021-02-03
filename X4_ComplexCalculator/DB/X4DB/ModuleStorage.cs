using System.Collections.Generic;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュールの保管庫情報
    /// </summary>
    public class ModuleStorage
    {
        #region スタティックメンバ
        /// <summary>
        /// モジュールの保管庫情報一覧
        /// </summary>
        private static readonly Dictionary<string, ModuleStorage> _ModuleStorages = new();


        /// <summary>
        /// ダミー用の保管庫情報一覧
        /// </summary>
        private static readonly Dictionary<string, ModuleStorage> _DummyStorages = new();


        /// <summary>
        /// ダミー用保管庫種別一覧
        /// </summary>
        private static readonly HashSet<TransportType> _DummyTypes = new();


        /// <summary>
        /// 保管庫種別の組み合わせ一覧
        /// </summary>
        private static readonly Dictionary<string, HashSet<TransportType>> _TransportTypeDict = new();
        #endregion



        #region プロパティ
        /// <summary>
        /// モジュールID
        /// </summary>
        public string ID { get; }


        /// <summary>
        /// 保管庫種別一覧
        /// </summary>
        public HashSet<TransportType> Types { get; }


        /// <summary>
        /// 容量
        /// </summary>
        public long Amount { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">モジュールID</param>
        /// <param name="amount">保管庫容量</param>
        /// <param name="types">保管庫種別一覧</param>
        private ModuleStorage(string id, long amount, string types)
        {
            ID = id;
            Amount = amount;
            Types = _TransportTypeDict[types];
        }


        /// <summary>
        /// ダミー用の保管庫情報コンストラクタ
        /// </summary>
        /// <param name="id">モジュールID</param>
        private ModuleStorage(string id)
        {
            ID = id;
            Types = _DummyTypes;
        }


        /// <summary>
        /// 初期化する
        /// </summary>
        public static void Init()
        {
            InitTransportTypeDict();

            _ModuleStorages.Clear();
            _DummyStorages.Clear();

            const string sql = @"
SELECT
	ModuleStorage.ModuleID,
	ModuleStorage.Amount,
	group_concat(Sorted_ModuleStorageType.TransportTypeID, '彁') AS TransportTypes
	
FROM
	ModuleStorage,
	(SELECT ModuleStorageType.ModuleID, ModuleStorageType.TransportTypeID FROM ModuleStorageType ORDER BY ModuleStorageType.ModuleID) Sorted_ModuleStorageType

WHERE
	ModuleStorage.ModuleID = Sorted_ModuleStorageType.ModuleID

GROUP BY
	ModuleStorage.ModuleID";

            foreach (var (id, amount, transportTypes) in X4Database.Instance.Query<(string, long, string)>(sql))
            {
                _ModuleStorages.Add(id, new ModuleStorage(id, amount, transportTypes));
            }
        }



        /// <summary>
        /// タグ一覧のディクショナリを初期化する
        /// </summary>
        private static void InitTransportTypeDict()
        {
            _TransportTypeDict.Clear();

            // Tagのユニークな組み合わせ一覧を取得する
            const string sql = @"
SELECT
	DISTINCT group_concat(Sorted_TransportTypeID.TransportTypeID, '彁') As TransportTypes
	
FROM
	(
		SELECT
			ModuleStorageType.ModuleID,
			ModuleStorageType.TransportTypeID
		FROM
			ModuleStorageType
		ORDER BY
			ModuleStorageType.ModuleID
	) Sorted_TransportTypeID

GROUP BY
	Sorted_TransportTypeID.ModuleID";

            foreach (var transportTypes in X4Database.Instance.Query<string>(sql))
            {
                var hashSet = new HashSet<TransportType>(transportTypes.Split('彁').Select(x => TransportType.Get(x)));
                _TransportTypeDict.Add(transportTypes, hashSet);
            }
        }


        /// <summary>
        /// 指定したモジュールIDに対応する保管庫情報を取得する
        /// </summary>
        /// <param name="id">モジュールID</param>
        /// <returns>指定したモジュールIDに対応する保管庫情報</returns>
        public static ModuleStorage Get(string id)
        {
            if (_ModuleStorages.TryGetValue(id, out var ret1))
            {
                return ret1;
            }

            if (_DummyStorages.TryGetValue(id, out var ret2))
            {
                return ret2;
            }

            var ret3 = new ModuleStorage(id);
            _DummyStorages.Add(id, ret3);
            return ret3;
        }
    }
}
