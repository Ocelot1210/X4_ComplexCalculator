using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// <see cref="EquipmentType"/> の一覧を管理するクラス
    /// </summary>
    class EquipmentTypeManager
    {
        #region メンバ
        /// <summary>
        /// 装備種別一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, EquipmentType> _EquipmentTypes;
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public EquipmentTypeManager(IDbConnection conn)
        {
            const string sql = "SELECT EquipmentTypeID, Name FROM EquipmentType";
            _EquipmentTypes = conn.Query<EquipmentType>(sql)
                .ToDictionary(x => x.EquipmentTypeID);
        }


        /// <summary>
        /// <paramref name="id"/> に対応する <see cref="EquipmentType"/> を取得する
        /// </summary>
        /// <param name="id">装備種別ID</param>
        /// <returns><paramref name="id"/> に対応する <see cref="EquipmentType"/></returns>
        /// <exception cref="KeyNotFoundException"><paramref name="id"/> に対応する <see cref="EquipmentType"/> が無い場合</exception>
        public EquipmentType Get(string id) => _EquipmentTypes[id];
    }
}
