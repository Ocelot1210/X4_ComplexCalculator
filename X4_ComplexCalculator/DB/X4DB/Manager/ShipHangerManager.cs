using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// 艦船のハンガー情報一覧を管理するクラス
    /// </summary>
    class ShipHangerManager
    {
        #region メンバ
        /// <summary>
        /// 艦船のハンガー一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, ShipHanger>> _ShipHangers;


        /// <summary>
        /// 空のハンガー情報(ダミー用)
        /// </summary>
        private readonly IReadOnlyDictionary<string, ShipHanger> _EmptyHanger = new Dictionary<string, ShipHanger>();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public ShipHangerManager(IDbConnection conn)
        {
            const string sql = @"SELECT ShipID, SizeID, Count, Capacity FROM ShipHanger";
            _ShipHangers = conn.Query<ShipHanger>(sql)
                .GroupBy(x => x.ShipID)
                .ToDictionary(
                    x => x.Key,
                    x => x.ToDictionary(y => y.Size.SizeID) as IReadOnlyDictionary<string, ShipHanger>);
        }


        /// <summary>
        /// 艦船IDに対応するハンガー情報を取得する
        /// </summary>
        /// <param name="id">艦船ID</param>
        /// <returns>艦船IDに対応するハンガー情報</returns>
        public IReadOnlyDictionary<string, ShipHanger> Get(string id) =>
            _ShipHangers.TryGetValue(id, out var hanger) ? hanger : _EmptyHanger;
    }
}
