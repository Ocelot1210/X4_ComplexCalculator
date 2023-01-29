using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IX4Module"/> に対応する <see cref="IModuleStorage"/> の一覧を管理するクラス
/// </summary>
class ModuleStorageManager
{
    #region メンバ
    /// <summary>
    /// モジュールの保管庫情報一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IModuleStorage> _moduleStorages;


    /// <summary>
    /// ダミー用の保管庫情報一覧
    /// </summary>
    private readonly Dictionary<string, IModuleStorage> _dummyStorages = new();


    /// <summary>
    /// ダミー用保管庫種別一覧
    /// </summary>
    private readonly HashSet<ITransportType> _dummyTypes = new();


    /// <summary>
    /// 保管庫種別の組み合わせ一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, HashSet<ITransportType>> _transportTypeDict;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    /// <param name="transportTypeManager">カーゴ種別一覧</param>
    public ModuleStorageManager(IDbConnection conn, TransportTypeManager transportTypeManager)
    {
        // Tagのユニークな組み合わせ一覧を作成する
        {
            const string SQL = @"
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

            _transportTypeDict = conn.Query<string>(SQL)
                .ToDictionary(x => x, x => new HashSet<ITransportType>(x.Split('彁').Select(y => transportTypeManager.Get(y))));
        }

        // モジュールの保管庫情報一覧を作成
        {
            const string SQL = @"
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

            _moduleStorages = conn.Query<(string ID, long Amount, string transportTypes)>(SQL)
                .ToDictionary(x => x.ID, x => new ModuleStorage(x.ID, x.Amount, _transportTypeDict[x.transportTypes]) as IModuleStorage);
        }
    }



    /// <summary>
    /// 指定したモジュールIDに対応する保管庫情報を取得する
    /// </summary>
    /// <param name="id">モジュールID</param>
    /// <returns>指定したモジュールIDに対応する保管庫情報</returns>
    public IModuleStorage Get(string id)
    {
        if (_moduleStorages.TryGetValue(id, out var ret1))
        {
            return ret1;
        }

        if (_dummyStorages.TryGetValue(id, out var ret2))
        {
            return ret2;
        }

        var ret3 = new ModuleStorage(id, 0, _dummyTypes);
        _dummyStorages.Add(id, ret3);
        return ret3;
    }
}
