using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// 派閥一覧を管理するクラス
    /// </summary>
    class FactionManager
    {
        #region メンバ
        /// <summary>
        /// 派閥一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, Faction> _Factions;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        /// <param name="raceManager">種族情報</param>
        public FactionManager(IDbConnection conn, RaceManager raceManager)
        {
            _Factions = conn.Query<X4_DataExporterWPF.Entity.Faction>("SELECT * FROM Faction")
                .Select(x => new Faction(x.FactionID, x.Name, raceManager.Get(x.RaceID)))
                .ToDictionary(x => x.FactionID);
        }


        /// <summary>
        /// 派閥IDに対応する派閥の取得を試みる
        /// </summary>
        /// <param name="id">種族ID</param>
        /// <returns>派閥IDに対応する派閥 派閥IDに対応する派閥が無ければnull</returns>
        public Faction? TryGet(string id) =>
            _Factions.TryGetValue(id, out var race) ? race : null;


        /// <summary>
        /// 派閥IDに対応する派閥を取得する
        /// </summary>
        /// <param name="id">派閥ID</param>
        /// <returns>派閥IDに対応する派閥</returns>
        /// <exception cref="KeyNotFoundException">派閥IDに対応する派閥が無い場合</exception>
        public Faction Get(string id) => _Factions[id];


        /// <summary>
        /// 全ての派閥を取得する
        /// </summary>
        /// <returns>全ての派閥情報の列挙</returns>
        public IEnumerable<Faction> GetAll() => _Factions.Values;
    }
}
