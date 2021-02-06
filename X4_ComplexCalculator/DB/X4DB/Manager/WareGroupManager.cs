using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// <see cref="WareGroup"/> の一覧を管理するクラス
    /// </summary>
    class WareGroupManager
    {
        #region メンバ
        /// <summary>
        /// ウェア種別IDをキーにした <see cref="WareGroup"/> の一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, WareGroup> _WareGroups;


        /// <summary>
        /// ダミー用ウェア種別
        /// </summary>
        private readonly WareGroup _DummyWareGroup = new("", "", -1);
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public WareGroupManager(IDbConnection conn)
        {
            const string sql = "SELECT WareGroupID, Name, Tier FROM WareGroup";
            _WareGroups = conn.Query<WareGroup>(sql)
                .ToDictionary(x => x.WareGroupID);
        }


        /// <summary>
        /// <paramref name="wareGroupID"/> に対応する <see cref="WareGroup"/> の取得を試みる
        /// </summary>
        /// <param name="wareGroupID">ウェア種別ID</param>
        /// <returns>
        /// <para><paramref name="wareGroupID"/> に対応する <see cref="WareGroup"/></para>
        /// <para>無ければ空の <see cref="WareGroup"/></para>
        /// </returns>
        public WareGroup TryGet(string wareGroupID) =>
            _WareGroups.TryGetValue(wareGroupID, out var ret) ? ret : _DummyWareGroup;
    }
}
