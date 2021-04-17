using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Entity;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// <see cref="IWareEffect"/> の一覧を管理するクラス
    /// </summary>
    class WareEffectManager
    {
        #region メンバ
        /// <summary>
        /// <see cref="IWare.ID"/> をキーにしたウェア生産時の追加効果情報一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, WareEffects> _WareEffects;


        /// <summary>
        /// 空のウェア生産時の追加効果情報
        /// </summary>
        private readonly IWareEffects _EmptyEffect = new WareEffects(Enumerable.Empty<IWareEffect>());
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">DB接続情報</param>
        public WareEffectManager(IDbConnection conn)
        {
            const string sql = "SELECT WareID, Method, EffectID, Product FROM WareEffect";

            _WareEffects = conn.Query<WareEffect>(sql)
                .GroupBy(x => x.WareID)
                .ToDictionary(x => x.Key, x => new WareEffects(x));
        }


        /// <summary>
        ///<see cref="IWare.ID"/> に対応するウェア生産時の追加効果情報一覧を取得する
        /// </summary>
        /// <param name="id"><see cref="IWare.ID"/></param>
        /// <returns><paramref name="id"/> に対応するウェア生産時の追加効果情報一覧</returns>
        public IWareEffects Get(string id)
            => _WareEffects.TryGetValue(id, out var effects) ? effects : _EmptyEffect;
    }
}
