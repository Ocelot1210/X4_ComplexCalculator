using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB.Manager
{
    /// <summary>
    /// <see cref="WareEffect"/> の一覧を管理するクラス
    /// </summary>
    class WareEffectManager
    {
        #region メンバ
        /// <summary>
        /// ウェア生産時の追加効果情報一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, WareEffects> _WareEffects;


        /// <summary>
        /// 空のウェア生産時の追加効果情報
        /// </summary>
        private readonly WareEffects _EmptyEffect = new(Enumerable.Empty<WareEffect>());
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
        /// <paramref name="id"/> に対応するウェア生産時の追加効果情報一覧を取得する
        /// </summary>
        /// <param name="id">ウェアID</param>
        /// <returns><paramref name="id"/> に対応するウェア生産時の追加効果情報一覧</returns>
        public WareEffects Get(string id)
        {
            if (_WareEffects.TryGetValue(id, out var effects))
            {
                return effects;
            }

            return _EmptyEffect;
        }
    }
}
