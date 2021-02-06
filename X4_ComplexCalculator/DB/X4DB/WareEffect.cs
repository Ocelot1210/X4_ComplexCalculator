using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// ウェア生産時の追加効果情報を管理するクラス
    /// </summary>
    public class WareEffect
    {
        #region スタティックメンバ
        /// <summary>
        /// ウェア生産時の追加効果情報一覧
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, WareEffect>>> _WareEffects = new();
        #endregion


        #region プロパティ
        /// <summary>
        /// ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// 生産方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 追加効果ID
        /// </summary>
        public string EffectID { get; }


        /// <summary>
        /// 追加効果の値
        /// </summary>
        public double Product { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">生産方式</param>
        /// <param name="effectID">追加効果ID</param>
        /// <param name="product">追加効果の値</param>
        public WareEffect(string wareID, string method, string effectID, double product)
        {
            WareID = wareID;
            Method = method;
            EffectID = effectID;
            Product = product;
        }
    }
}
