using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// ウェアの生産量と生産時間一覧を管理するクラス
    /// </summary>
    class WareProductionManager
    {
        #region メンバ
        /// <summary>
        /// ウェアの生産量と生産時間情報の一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, WareProduction>> _WareProductions;


        /// <summary>
        /// ダミー用ウェア生産情報
        /// </summary>
        private readonly IReadOnlyDictionary<string, WareProduction> _DummyWareProduction = new Dictionary<string, WareProduction>();
        #endregion



        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public WareProductionManager(IDbConnection conn)
        {
            const string sql = "SELECT WareID, Method, Name, Amount, Time FROM WareProduction";

            _WareProductions = conn.Query<WareProduction>(sql)
                .GroupBy(x => x.WareID)
                .ToDictionary(
                    x => x.Key,
                    x => x.ToDictionary(y => y.Method) as IReadOnlyDictionary<string, WareProduction>
                );
        }



        /// <summary>
        /// ウェアIDに対応するウェア生産情報一覧を取得
        /// </summary>
        /// <param name="id">ウェアID</param>
        /// <returns>ウェアIDに対応するウェア生産方式一覧</returns>
        public IReadOnlyDictionary<string, WareProduction> Get(string id)
            => _WareProductions.TryGetValue(id, out var ret) ? ret : _DummyWareProduction;



        /// <summary>
        /// ウェアIDと生産方式に対応する生産情報を取得する
        /// </summary>
        /// <param name="id">ウェアID</param>
        /// <param name="method">生産方式</param>
        /// <returns>ウェアIDと生産方式に対応する生産情報</returns>
        public WareProduction Get(string id, string method)
        {
            var productions = Get(id);

            if (productions.TryGetValue(method, out var production))
            {
                return production;
            }

            return productions["default"];
        }
    }
}
