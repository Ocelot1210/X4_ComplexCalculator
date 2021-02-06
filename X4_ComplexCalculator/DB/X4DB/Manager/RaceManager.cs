using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// <see cref="Race"/> の一覧を管理するクラス
    /// </summary>
    public class RaceManager
    {
        #region メンバ
        /// <summary>
        /// 種族IDをキーにした <see cref="Race"/> の一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, Race> _Races;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public RaceManager(IDbConnection conn)
        {
            const string sql = "SELECT RaceID, Name, ShortName, Description FROM Race";
            _Races = conn.Query<Race>(sql).ToDictionary(x => x.RaceID);
        }


        /// <summary>
        /// <paramref name="id"/> に対応する <see cref="Race"/> の取得を試みる
        /// </summary>
        /// <param name="id">種族ID</param>
        /// <returns>
        /// <para><paramref name="id"/> に対応する <see cref="Race"/></para>
        /// <para><paramref name="id"/> に対応する <see cref="Race"/> が無ければ null</para></returns>
        public Race? TryGet(string id) =>
            _Races.TryGetValue(id, out var race) ? race : null;


        /// <summary>
        /// <paramref name="id"/> に対応する <see cref="Race"/> を取得する
        /// </summary>
        /// <param name="id">種族ID</param>
        /// <returns><paramref name="id"/> に対応する <see cref="Race"/></returns>
        /// <exception cref="KeyNotFoundException"><paramref name="id"/> に対応する <see cref="Race"/> が無い場合</exception>
        public Race Get(string id) => _Races[id];


        /// <summary>
        /// 全ての <see cref="Race"/> を取得する
        /// </summary>
        /// <returns>全ての <see cref="Race"/> の列挙</returns>
        public IEnumerable<Race> GetAll() => _Races.Values;
    }
}
