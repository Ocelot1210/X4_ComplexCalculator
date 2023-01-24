using Dapper;
using System.Collections.Generic;
using System.Data;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.DB.X4DB.Manager;

namespace X4_ComplexCalculator.DB.X4DB.Builder;

/// <summary>
/// <see cref="Ware"/> クラスのインスタンスを作成するBuilderクラス
/// </summary>
class WareBuilder
{
    #region メンバ
    /// <summary>
    /// DB接続情報
    /// </summary>
    private readonly IDbConnection _Conn;


    /// <summary>
    /// カーゴ種別一覧
    /// </summary>
    private readonly TransportTypeManager _TransportTypeManager;


    /// <summary>
    /// タグ情報一覧
    /// </summary>
    private readonly WareTagsManager _WareTagsManager;


    /// <summary>
    /// ウェア所有派閥情報一覧
    /// </summary>
    private readonly WareOwnerManager _WareOwnerManager;


    /// <summary>
    /// ウェア生産に必要なウェア情報一覧
    /// </summary>
    private readonly WareResourceManager _WareResourceManager;


    /// <summary>
    /// ウェアの生産量と生産時間一覧
    /// </summary>
    private readonly WareProductionManager _WareProductionManager;


    /// <summary>
    /// ウェア種別一覧
    /// </summary>
    private readonly WareGroupManager _WareGroupManager;


    /// <summary>
    /// ウェア生産時の追加効果一覧
    /// </summary>
    private readonly WareEffectManager _WareEffectManager;


    /// <summary>
    /// 艦船情報ビルダ
    /// </summary>
    private readonly ShipBuilder _ShipBuilder;


    /// <summary>
    /// モジュール情報ビルダ
    /// </summary>
    private readonly ModuleBuilder _ModuleBuilder;


    /// <summary>
    /// 装備情報ビルダ
    /// </summary>
    private readonly EquipmentBuilder _EquipmentBuilder;
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public WareBuilder(IDbConnection conn, TransportTypeManager transportTypeManager)
    {
        _Conn = conn;

        _TransportTypeManager = transportTypeManager;
        _WareTagsManager = new(conn);
        _WareOwnerManager = new(conn);
        _WareResourceManager = new(conn);
        _WareProductionManager = new(conn);
        _WareGroupManager = new(conn);
        _WareEffectManager = new(conn);


        var wareEquipmentManager = new WareEquipmentManager(conn);

        _ShipBuilder = new(conn, wareEquipmentManager);
        _ModuleBuilder = new(conn, _WareProductionManager, transportTypeManager, wareEquipmentManager);
        _EquipmentBuilder = new(conn);
    }



    /// <summary>
    /// 全ウェア情報を作成する
    /// </summary>
    /// <returns>全ウェア情報の列挙</returns>
    public IEnumerable<IWare> BuildAll()
    {
        const string sql1 = @"
SELECT *
FROM   Ware
WHERE  TransportTypeID IS NOT NULL AND TransportTypeID <> 'inventory'";

        foreach (var item in _Conn.Query<X4_DataExporterWPF.Entity.Ware>(sql1))
        {
            var ware = new Ware(
                item.WareID,
                item.Name,
                _WareGroupManager.TryGet(item.WareGroupID ?? ""),
                _TransportTypeManager.Get(item.TransportTypeID!),
                item.Description,
                item.Volume,
                item.MinPrice,
                item.AvgPrice,
                item.MaxPrice,
                _WareOwnerManager.Get(item.WareID),
                _WareProductionManager.Get(item.WareID),
                _WareResourceManager.Get(item.WareID),
                _WareTagsManager.Get(item.WareID),
                _WareEffectManager.Get(item.WareID)
            );


            // ステーションモジュールの場合
            if (ware.Tags.Contains("module"))
            {
                yield return _ModuleBuilder.Builld(ware);
                continue;
            }

            // 装備の場合
            if (ware.Tags.Contains("equipment"))
            {
                yield return _EquipmentBuilder.Build(ware);
                continue;
            }

            // 艦船の場合
            if (ware.Tags.Contains("ship"))
            {
                yield return _ShipBuilder.Build(ware);
                continue;
            }

            // それ以外の場合
            yield return ware;
        }

        yield break;
    }
}
