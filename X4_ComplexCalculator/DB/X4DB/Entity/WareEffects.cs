using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// ウェア単位のウェア生産時の追加効果情報用クラス
    /// </summary>
    public class WareEffects : IWareEffects
    {
        #region メンバ
        /// <summary>
        /// ウェア生産時の追加効果情報一覧
        /// </summary>
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IWareEffect>> _Effects;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="effects">ウェア単位の追加効果情報一覧</param>
        public WareEffects(IEnumerable<IWareEffect> effects)
        {
            _Effects = effects
                .GroupBy(x => x.Method)
                .ToDictionary(
                    x => x.Key,
                    x => x.ToDictionary(y => y.EffectID) as IReadOnlyDictionary<string, IWareEffect>
                );
        }


        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IWareEffect>? TryGet(string method)
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


        /// <inheritdoc/>
        public IWareEffect? TryGet(string method, string effectID)
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
