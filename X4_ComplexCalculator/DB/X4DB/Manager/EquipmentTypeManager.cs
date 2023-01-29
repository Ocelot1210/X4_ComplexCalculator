using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager;

/// <summary>
/// <see cref="IEquipment"/> に対応する <see cref="IEquipmentType"/> を管理するクラス
/// </summary>
class EquipmentTypeManager
{
    #region メンバ
    /// <summary>
    /// 装備種別一覧
    /// </summary>
    private readonly IReadOnlyDictionary<string, IEquipmentType> _equipmentTypes;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="conn">DB接続情報</param>
    public EquipmentTypeManager(IDbConnection conn)
    {
        const string SQL = "SELECT EquipmentTypeID, Name FROM EquipmentType";
        _equipmentTypes = conn.Query<EquipmentType>(SQL)
            .ToDictionary(x => x.EquipmentTypeID, x => x as IEquipmentType);
    }


    /// <summary>
    /// <paramref name="id"/> に対応する <see cref="IEquipmentType"/> を取得する
    /// </summary>
    /// <param name="id">装備種別ID</param>
    /// <returns><paramref name="id"/> に対応する <see cref="IEquipmentType"/></returns>
    /// <exception cref="KeyNotFoundException"><paramref name="id"/> に対応する <see cref="IEquipmentType"/> が無い場合</exception>
    public IEquipmentType Get(string id) => _equipmentTypes[id];
}
