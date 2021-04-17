using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB.Interfaces
{
    /// <summary>
    /// ウェア単位のウェア生産時の追加効果情報用インターフェイス
    /// </summary>
    public interface IWareEffects
    {
        /// <summary>
        /// <paramref name="method"/> に対応する追加効果情報一覧を取得する
        /// </summary>
        /// <param name="method">ウェア生産方式</param>
        /// <returns><paramref name="method"/> に対応する追加効果情報一覧 又は null</returns>
        public IReadOnlyDictionary<string, IWareEffect>? TryGet(string method);



        /// <summary>
        /// <paramref name="method"/> と <paramref name="effectID"/> に対応する追加効果情報を取得する
        /// </summary>
        /// <param name="method">ウェア生産方式</param>
        /// <param name="effectID">追加効果ID</param>
        /// <returns></returns>
        public IWareEffect? TryGet(string method, string effectID);
    }
}
