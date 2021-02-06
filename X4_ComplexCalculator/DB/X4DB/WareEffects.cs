using System.Collections.Generic;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// ウェア単位のウェア生産時の追加効果情報
    /// </summary>
    public class WareEffects
    {
        #region メンバ
        /// <summary>
        /// ウェア生産時の追加効果情報一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, WareEffect>> _Effects;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="effects">ウェア単位の追加効果情報一覧</param>
        public WareEffects(IEnumerable<WareEffect> effects)
        {
            _Effects = effects
                .GroupBy(x => x.Method)
                .ToDictionary(
                    x => x.Key,
                    x => x.ToDictionary(y => y.EffectID) as IReadOnlyDictionary<string, WareEffect>
                );
        }


        /// <summary>
        /// <paramref name="method"/> に対応する追加効果情報一覧を取得する
        /// </summary>
        /// <param name="method">ウェア生産方式</param>
        /// <returns><paramref name="method"/> に対応する追加効果情報一覧 又は null</returns>
        public IReadOnlyDictionary<string, WareEffect>? TryGet(string method)
        {
            // 生産方式で絞り込み
            if (!_Effects.TryGetValue(method, out var effects))
            {
                // デフォルトの生産方式で取得
                if (!_Effects.TryGetValue("default", out effects))
                {
                    // 生産方式取得失敗
                    return null;
                }
            }

            return effects;
        }


        /// <summary>
        /// <paramref name="method"/> と <paramref name="effectID"/> に対応する追加効果情報を取得する
        /// </summary>
        /// <param name="method">ウェア生産方式</param>
        /// <param name="effectID">追加効果ID</param>
        /// <returns></returns>
        public WareEffect? TryGet(string method, string effectID)
        {
            var effects = TryGet(method);
            if (effects is not null)
            {
                return effects.TryGetValue(effectID, out var effect) ? effect : null;
            }

            return null;
        }
    }
}
