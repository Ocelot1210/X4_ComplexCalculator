using Collections.Pooled;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;


namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IX4Module"/> に対応する <see cref="IModuleStorage"/> の一覧を管理するクラス
/// </summary>
sealed class ModuleStorageManager : IDisposable
{
    #region メンバ
    /// <summary>
    /// モジュールの保管庫情報一覧
    /// </summary>
    private readonly PooledDictionary<string, IModuleStorage> _moduleStorages;


    /// <summary>
    /// ダミー用の保管庫情報一覧
    /// </summary>
    private readonly PooledDictionary<string, IModuleStorage> _dummyStorages = new();


    /// <summary>
    /// ダミー用保管庫種別一覧
    /// </summary>
    private readonly HashSet<ITransportType> _dummyTypes = new();
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    /// <param name="transportTypeManager">カーゴ種別一覧</param>
    public ModuleStorageManager(IDbConnection conn, TransportTypeManager transportTypeManager)
    {
        // モジュールの保管庫情報一覧を作成
        const string SQL = @"
WITH
	TmpStorageType AS (
		SELECT   ModuleID, TransportTypeID
		FROM     ModuleStorageType
		ORDER BY ModuleID, TransportTypeID
	)

SELECT   M.ModuleID, M.Amount, group_concat(T.TransportTypeID, '彁') AS TransportTypes
FROM     ModuleStorage M, TmpStorageType T
WHERE    M.ModuleID = T.ModuleID
GROUP BY M.ModuleID
";
        int capacity = conn.QuerySingle<int>("SELECT count(*) FROM (SELECT DISTINCT * FROM ModuleStorageType)");

        using var transportTypeMgr = new TagsManager<HashSet<ITransportType>>(capacity, x => [.. x.Split('彁').Select(X4Database.Instance.TransportType.Get)]);

        _moduleStorages = new(capacity);
        foreach (var (id, amount, transportTypes) in conn.Query<(string, long, string)>(SQL))
        {
            _moduleStorages.Add(id, new ModuleStorage(id, amount, transportTypeMgr[transportTypes]));
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


    /// <inheritdoc/>
    public void Dispose()
    {
        _moduleStorages.Dispose();
        _dummyStorages.Dispose();
    }
}
