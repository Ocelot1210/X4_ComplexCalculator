using Collections.Pooled;
using Dapper;
using System;
using System.Data;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Builder;

/// <summary>
/// <see cref="Thruster"/> クラスのインスタンスを作成するBuilderクラス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="conn">DB接続情報</param>
sealed class ThrusterBuilder(IDbConnection conn) : IDisposable
{
    #region メンバ
    /// <summary>
    /// スラスター情報一覧
    /// </summary>
    private readonly PooledDictionary<string, X4_DataExporterWPF.Entities.Thruster> _thrusters = 
        conn.Query<X4_DataExporterWPF.Entities.Thruster>("SELECT * FROM Thruster")
            .ToPooledDictionary(x => x.EquipmentID);
    #endregion


    /// <summary>
    /// スラスター情報作成
    /// </summary>
    /// <param name="equipment">ベースとなる装備情報</param>
    /// <returns>スラスター情報または装備情報</returns>
    public IEquipment Build(IEquipment equipment)
    {
        if (_thrusters.TryGetValue(equipment.ID, out var item))
        {
            return new Thruster(
                equipment,
                item.ThrustStrafe,
                item.ThrustPitch,
                item.ThrustYaw,
                item.ThrustRoll,
                item.AngularRoll,
                item.AngularPitch
            );
        }

        return equipment;
    }


    /// <inheritdoc/>
    public void Dispose()
    {
        _thrusters.Dispose();
    }
}
