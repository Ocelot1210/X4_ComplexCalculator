using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// <see cref="TransportType"/> の一覧を管理するクラス
    /// </summary>
    public class TransportTypeManager
    {
        /// <summary>
        /// カーゴタイプ一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, TransportType> _TransportTypes;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public TransportTypeManager(IDbConnection conn)
        {
            const string sql = "SELECT TransportTypeID, Name FROM TransportType";
            _TransportTypes = conn.Query<TransportType>(sql).ToDictionary(x => x.TransportTypeID);
        }


        /// <summary>
        /// <paramref name="id"/> に対応する <see cref="TransportType"/> を取得する
        /// </summary>
        /// <param name="id">カーゴ種別ID</param>
        /// <returns><paramref name="id"/> に対応する <see cref="TransportType"/></returns>
        /// <exception cref="KeyNotFoundException"><paramref name="id"/> に対応する <see cref="TransportType"/> が無い場合</exception>
        public TransportType Get(string id) => _TransportTypes[id];


        /// <summary>
        /// 全ての <see cref="TransportType"/> を列挙する
        /// </summary>
        /// <returns>全ての <see cref="TransportType"/> の列挙</returns>
        public IEnumerable<TransportType> GetAll() => _TransportTypes.Values;
    }
}
