using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.DB.X4DB.Manager;

namespace X4_ComplexCalculator.DB.X4DB.Builder;

/// <summary>
/// <see cref="Module"/> クラスのインスタンスを作成するBuilderクラス
/// </summary>
class ModuleBuilder
{
    #region メンバ
    /// <summary>
    /// モジュール種別一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IModuleType> _moduleTypes;


    /// <summary>
    /// モジュール情報一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, X4_DataExporterWPF.Entity.Module> _modules;


    /// <summary>
    /// モジュールの製品情報一覧
    /// </summary>
    private readonly ModuleProductManager _moduleProductManager;


    /// <summary>
    /// モジュールの保管庫情報管理
    /// </summary>
    private readonly ModuleStorageManager _storageManager;


    /// <summary>
    /// ウェアの装備情報一覧
    /// </summary>
    private readonly WareEquipmentManager _wareEquipmentManager;
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    /// <param name="wareProductionManager">ウェア生産情報一覧</param>
    /// <param name="transportTypeManager">カーゴ種別一覧</param>
    public ModuleBuilder(
        IDbConnection conn,
        WareProductionManager wareProductionManager,
        TransportTypeManager transportTypeManager,
        WareEquipmentManager wareEquipmentManager
    )
    {
        // モジュール種別一覧を作成
        {
            const string SQL = "SELECT ModuleTypeID, Name FROM ModuleType";
            _moduleTypes = conn.Query<ModuleType>(SQL)
                .ToDictionary(x => x.ModuleTypeID, x => x as IModuleType);
        }


        // モジュール情報一覧を作成
        _modules = conn.Query<X4_DataExporterWPF.Entity.Module>("SELECT * FROM Module")
            .ToDictionary(x => x.ModuleID);


        _moduleProductManager = new(conn, wareProductionManager);

        _storageManager = new(conn, transportTypeManager);

        _wareEquipmentManager = wareEquipmentManager;
    }


    /// <summary>
    /// モジュール情報作成
    /// </summary>
    /// <param name="ware">ベースとなるウェア情報</param>
    /// <returns>モジュール情報</returns>
    public IWare Builld(IWare ware)
    {
        if (!ware.Tags.Contains("module"))
        {
            throw new ArgumentException("Ware is not Module", nameof(ware));
        }

        if (!_modules.TryGetValue(ware.ID, out var item))
        {
            return ware;
        }

        return new Module(
            ware,
            item.Macro,
            _moduleTypes[item.ModuleTypeID],
            item.MaxWorkers,
            item.WorkersCapacity,
            item.NoBlueprint,
            _moduleProductManager.Get(ware.ID),
            _storageManager.Get(ware.ID),
            _wareEquipmentManager.Get(ware.ID).ToDictionary(x => x.ConnectionName)
        );
    }
}
