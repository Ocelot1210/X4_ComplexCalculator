using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Builder;

/// <summary>
/// <see cref="Shield"/> クラスのインスタンスを作成するBuilderクラス
/// </summary>
class ShieldBuilder
{
    #region メンバ
    /// <summary>
    /// シールド情報一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, X4_DataExporterWPF.Entity.Shield> _Shields;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public ShieldBuilder(IDbConnection conn)
    {
        _Shields = conn.Query<X4_DataExporterWPF.Entity.Shield>("SELECT * FROM Shield")
            .ToDictionary(x => x.EquipmentID);
    }


    /// <summary>
    /// シールド情報作成
    /// </summary>
    /// <param name="equipment">ベースとなる装備情報</param>
    /// <returns>シールド情報または装備情報</returns>
    public IEquipment Build(IEquipment equipment)
    {
        if (_Shields.TryGetValue(equipment.ID, out var item))
        {
            return new Shield(
                equipment,
                item.Capacity,
                item.RechargeRate,
                item.RechargeDelay
            );
        }

        return equipment;
    }
}
