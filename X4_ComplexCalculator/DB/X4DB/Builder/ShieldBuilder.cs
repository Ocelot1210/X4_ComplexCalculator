using Collections.Pooled;
using Dapper;
using System;
using System.Data;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Builder;

/// <summary>
/// <see cref="Shield"/> クラスのインスタンスを作成するBuilderクラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="conn">DB接続情報</param>
sealed class ShieldBuilder(IDbConnection conn) : IDisposable
{
    #region メンバ
    /// <summary>
    /// シールド情報一覧
    /// </summary>
    private readonly PooledDictionary<string, X4_DataExporterWPF.Entities.Shield> _shields = 
        conn.Query<X4_DataExporterWPF.Entities.Shield>("SELECT * FROM Shield")
            .ToPooledDictionary(x => x.EquipmentID);
    #endregion


    /// <summary>
    /// シールド情報作成
    /// </summary>
    /// <param name="equipment">ベースとなる装備情報</param>
    /// <returns>シールド情報または装備情報</returns>
    public IEquipment Build(IEquipment equipment)
    {
        if (_shields.TryGetValue(equipment.ID, out var item))
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


    /// <inheritdoc/>
    public void Dispose()
    {
        _shields.Dispose();
    }
}
