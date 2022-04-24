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
    private readonly IReadOnlyDictionary<string, IModuleType> _ModuleTypes;


    /// <summary>
    /// モジュール情報一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, X4_DataExporterWPF.Entity.Module> _Modules;


    /// <summary>
    /// モジュールの製品情報一覧
    /// </summary>
    private readonly ModuleProductManager _ModuleProductManager;


    /// <summary>
    /// モジュールの保管庫情報管理
    /// </summary>
    private readonly ModuleStorageManager _StorageManager;


    /// <summary>
    /// ウェアの装備情報一覧
    /// </summary>
    private WareEquipmentManager _WareEquipmentManager;
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
            const string sql = "SELECT ModuleTypeID, Name FROM ModuleType";
            _ModuleTypes = conn.Query<ModuleType>(sql)
                .ToDictionary(x => x.ModuleTypeID, x => x as IModuleType);
        }


        // モジュール情報一覧を作成
        _Modules = conn.Query<X4_DataExporterWPF.Entity.Module>("SELECT * FROM Module")
            .ToDictionary(x => x.ModuleID);


        _ModuleProductManager = new(conn, wareProductionManager);

        _StorageManager = new(conn, transportTypeManager);

        _WareEquipmentManager = wareEquipmentManager;
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
            throw new ArgumentException();
        }

        if (!_Modules.TryGetValue(ware.ID, out var item))
        {
            return ware;
        }

        return new Module(
            ware,
            item.Macro,
            _ModuleTypes[item.ModuleTypeID],
            item.MaxWorkers,
            item.WorkersCapacity,
            item.NoBlueprint,
            _ModuleProductManager.Get(ware.ID),
            _StorageManager.Get(ware.ID),
            _WareEquipmentManager.Get(ware.ID).ToDictionary(x => x.ConnectionName)
        );
    }
}
