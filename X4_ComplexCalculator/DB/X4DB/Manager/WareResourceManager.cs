using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// ウェア生産に必要なウェア情報一覧を管理する
    /// </summary>
    class WareResourceManager
    {
        #region メンバ
        /// <summary>
        /// 1サイクルのウェア生産に必要なウェア情報一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<WareResource>>> _WareResources;


        /// <summary>
        /// ダミー用1サイクルのウェア生産に必要なウェア情報一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, IReadOnlyList<WareResource>> _DummyResources
            = new Dictionary<string, IReadOnlyList<WareResource>>();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public WareResourceManager(IDbConnection conn)
        {
            // ウェア生産情報を作成する
            {
                const string sql = "SELECT WareID, Method, NeedWareID, Amount FROM WareResource";

                _WareResources = conn.Query<WareResource>(sql)
                    .GroupBy(x => x.WareID)
                    .ToDictionary(
                        x => x.Key,
                        x => x.GroupBy(y => y.Method).ToDictionary(y => y.Key, y => y.ToArray() as IReadOnlyList<WareResource>) as IReadOnlyDictionary<string, IReadOnlyList<WareResource>>
                    );
            }
        }


        /// <summary>
        /// ウェアIDに対応するウェア生産に必要なウェア情報一覧を取得する
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <returns>ウェアIDに対応するウェア生産に必要なウェア情報一覧</returns>
        public IReadOnlyDictionary<string, IReadOnlyList<WareResource>> Get(string wareID)
            => _WareResources.TryGetValue(wareID, out var resources) ? resources : _DummyResources;
    }
}
