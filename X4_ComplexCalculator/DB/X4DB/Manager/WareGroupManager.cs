using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// <see cref="IWareGroup"/> の一覧を管理するクラス
    /// </summary>
    class WareGroupManager
    {
        #region メンバ
        /// <summary>
        /// ウェア種別IDをキーにした <see cref="IWareGroup"/> の一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, IWareGroup> _WareGroups;


        /// <summary>
        /// ダミー用ウェア種別
        /// </summary>
        private readonly IWareGroup _DummyWareGroup = new WareGroup("", "", -1);
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public WareGroupManager(IDbConnection conn)
        {
            const string sql = "SELECT WareGroupID, Name, Tier FROM WareGroup";
            _WareGroups = conn.Query<WareGroup>(sql)
                .ToDictionary(x => x.WareGroupID, x => x as IWareGroup);
        }


        /// <summary>
        /// <paramref name="wareGroupID"/> に対応する <see cref="IWareGroup"/> の取得を試みる
        /// </summary>
        /// <param name="wareGroupID">ウェア種別ID</param>
        /// <returns>
        /// <para><paramref name="wareGroupID"/> に対応する <see cref="IWareGroup"/></para>
        /// <para>無ければ空の <see cref="IWareGroup"/></para>
        /// </returns>
        public IWareGroup TryGet(string wareGroupID) =>
            _WareGroups.TryGetValue(wareGroupID, out var ret) ? ret : _DummyWareGroup;
    }
}
