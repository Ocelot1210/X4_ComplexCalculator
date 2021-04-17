using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// <see cref="ITransportType"/> の一覧を管理するクラス
    /// </summary>
    public class TransportTypeManager
    {
        /// <summary>
        /// カーゴタイプ一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, ITransportType> _TransportTypes;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public TransportTypeManager(IDbConnection conn)
        {
            const string sql = "SELECT TransportTypeID, Name FROM TransportType";
            _TransportTypes = conn.Query<TransportType>(sql).ToDictionary(x => x.TransportTypeID, x => x as ITransportType);
        }


        /// <summary>
        /// <paramref name="id"/> に対応する <see cref="ITransportType"/> を取得する
        /// </summary>
        /// <param name="id">カーゴ種別ID</param>
        /// <returns><paramref name="id"/> に対応する <see cref="ITransportType"/></returns>
        /// <exception cref="KeyNotFoundException"><paramref name="id"/> に対応する <see cref="ITransportType"/> が無い場合</exception>
        public ITransportType Get(string id) => _TransportTypes[id];


        /// <summary>
        /// 全ての <see cref="ITransportType"/> を列挙する
        /// </summary>
        /// <returns>全ての <see cref="ITransportType"/> の列挙</returns>
        public IEnumerable<ITransportType> GetAll() => _TransportTypes.Values;
    }
}
