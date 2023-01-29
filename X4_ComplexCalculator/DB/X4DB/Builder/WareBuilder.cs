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
    private readonly IDbConnection _conn;


    /// <summary>
    /// カーゴ種別一覧
    /// </summary>
    private readonly TransportTypeManager _transportTypeManager;


    /// <summary>
    /// タグ情報一覧
    /// </summary>
    private readonly WareTagsManager _wareTagsManager;


    /// <summary>
    /// ウェア所有派閥情報一覧
    /// </summary>
    private readonly WareOwnerManager _wareOwnerManager;


    /// <summary>
    /// ウェア生産に必要なウェア情報一覧
    /// </summary>
    private readonly WareResourceManager _wareResourceManager;


    /// <summary>
    /// ウェアの生産量と生産時間一覧
    /// </summary>
    private readonly WareProductionManager _wareProductionManager;


    /// <summary>
    /// ウェア種別一覧
    /// </summary>
    private readonly WareGroupManager _wareGroupManager;


    /// <summary>
    /// ウェア生産時の追加効果一覧
    /// </summary>
    private readonly WareEffectManager _wareEffectManager;


    /// <summary>
    /// 艦船情報ビルダ
    /// </summary>
    private readonly ShipBuilder _shipBuilder;


    /// <summary>
    /// モジュール情報ビルダ
    /// </summary>
    private readonly ModuleBuilder _moduleBuilder;


    /// <summary>
    /// 装備情報ビルダ
    /// </summary>
    private readonly EquipmentBuilder _equipmentBuilder;
    #endregion



    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public WareBuilder(IDbConnection conn, TransportTypeManager transportTypeManager)
    {
        _conn = conn;

        _transportTypeManager = transportTypeManager;
        _wareTagsManager = new(conn);
        _wareOwnerManager = new(conn);
        _wareResourceManager = new(conn);
        _wareProductionManager = new(conn);
        _wareGroupManager = new(conn);
        _wareEffectManager = new(conn);


        var wareEquipmentManager = new WareEquipmentManager(conn);

        _shipBuilder = new(conn, wareEquipmentManager);
        _moduleBuilder = new(conn, _wareProductionManager, transportTypeManager, wareEquipmentManager);
        _equipmentBuilder = new(conn);
    }



    /// <summary>
    /// 全ウェア情報を作成する
    /// </summary>
    /// <returns>全ウェア情報の列挙</returns>
    public IEnumerable<IWare> BuildAll()
    {
        const string SQL_1 = @"
SELECT *
FROM   Ware
WHERE  TransportTypeID IS NOT NULL AND TransportTypeID <> 'inventory'";

        foreach (var item in _conn.Query<X4_DataExporterWPF.Entity.Ware>(SQL_1))
        {
            var ware = new Ware(
                item.WareID,
                item.Name,
                _wareGroupManager.TryGet(item.WareGroupID ?? ""),
                _transportTypeManager.Get(item.TransportTypeID!),
                item.Description,
                item.Volume,
                item.MinPrice,
                item.AvgPrice,
                item.MaxPrice,
                _wareOwnerManager.Get(item.WareID),
                _wareProductionManager.Get(item.WareID),
                _wareResourceManager.Get(item.WareID),
                _wareTagsManager.Get(item.WareID),
                _wareEffectManager.Get(item.WareID)
            );


            // ステーションモジュールの場合
            if (ware.Tags.Contains("module"))
            {
                yield return _moduleBuilder.Builld(ware);
                continue;
            }

            // 装備の場合
            if (ware.Tags.Contains("equipment"))
            {
                yield return _equipmentBuilder.Build(ware);
                continue;
            }

            // 艦船の場合
            if (ware.Tags.Contains("ship"))
            {
                yield return _shipBuilder.Build(ware);
                continue;
            }

            // それ以外の場合
            yield return ware;
        }

        yield break;
    }
}
