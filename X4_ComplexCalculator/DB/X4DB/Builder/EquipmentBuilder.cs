using Collections.Pooled;
using Dapper;
using System;
using System.Data;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;
using X4_ComplexCalculator.DB.X4DB.Manager;

namespace X4_ComplexCalculator.DB.X4DB.Builder;

/// <summary>
/// <see cref="Equipment"/> クラスのインスタンスを作成するBuilderクラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="conn"></param>
sealed class EquipmentBuilder(IDbConnection conn) : IDisposable
{
    #region メンバ
    /// <summary>
    /// タグ情報一覧
    /// </summary>
    private readonly EquipmentTagsManager _equipmentTagsManager = new(conn);


    /// <summary>
    /// <see cref="IEngine"/> 情報ビルダ
    /// </summary>
    private readonly EngineBuilder _engineBuilder = new(conn);


    /// <summary>
    /// <see cref="IShield"/> 情報ビルダ
    /// </summary>
    private readonly ShieldBuilder _shieldBuilder = new(conn);


    /// <summary>
    /// <see cref="IThruster"/> 情報ビルダ
    /// </summary>
    private readonly ThrusterBuilder _thrusterBuilder = new(conn);


    /// <summary>
    /// 装備一覧
    /// </summary>
    private readonly PooledDictionary<string, X4_DataExporterWPF.Entities.Equipment> _equipments = conn.Query<X4_DataExporterWPF.Entities.Equipment>("SELECT * FROM Equipment")
        .ToPooledDictionary(x => x.EquipmentID);
    #endregion


    /// <summary>
    /// 装備情報作成
    /// </summary>
    /// <param name="ware">ベースとなるウェア情報</param>
    /// <returns>装備情報</returns>
    public IWare Build(IWare ware)
    {
        if (!ware.Tags.Contains("equipment"))
        {
            throw new ArgumentException("Ware is not Equipment", nameof(ware));
        }

        if (_equipments.TryGetValue(ware.ID, out var item))
        {
            var ret = new Equipment(
                ware,
                item.MacroName,
                X4Database.Instance.EquipmentType.Get(item.EquipmentTypeID),
                item.Hull,
                item.HullIntegrated,
                item.Mk,
                X4Database.Instance.Race.TryGet(item.MakerRace ?? ""),
                _equipmentTagsManager.GetTags(ware.ID),
                _equipmentTagsManager.TryGetSize(ware.ID)
            );


            return ret.EquipmentType.EquipmentTypeID switch
            {
                "engines"   => _engineBuilder.Build(ret),
                "shields"   => _shieldBuilder.Build(ret),
                "thrusters" => _thrusterBuilder.Build(ret),
                _           => ret,
            };
        }
        else
        {
            return ware;
        }
    }


    /// <inheritdoc/>
    public void Dispose()
    {
        _equipments.Dispose();
        _equipmentTagsManager.Dispose();
        _engineBuilder.Dispose();
        _shieldBuilder.Dispose();
        _thrusterBuilder.Dispose();
    }
}
