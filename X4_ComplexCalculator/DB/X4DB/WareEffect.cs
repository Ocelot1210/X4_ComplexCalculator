using System;
using System.Collections.Generic;
using System.Text;

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
        private static Dictionary<string, Dictionary<string, Dictionary<string, WareEffect>>> _WareEffects = new();
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
        private WareEffect(string wareID, string method, string effectID, double product)
        {
            WareID = wareID;
            Method = method;
            EffectID = effectID;
            Product = product;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _WareEffects.Clear();

            const string sql = "SELECT WareID, Method, EffectID, Product FROM WareEffect";
            foreach (var item in X4Database.Instance.Query<WareEffect>(sql))
            {
                if (!_WareEffects.ContainsKey(item.WareID))
                {
                    _WareEffects.Add(item.WareID, new Dictionary<string, Dictionary<string, WareEffect>>());
                }

                var methods = _WareEffects[item.WareID];
                if (!methods.ContainsKey(item.Method))
                {
                    methods.Add(item.Method, new Dictionary<string, WareEffect>());
                }

                _WareEffects[item.WareID][item.Method].Add(item.EffectID, item);
            }
        }


        /// <summary>
        /// ウェア生産時の追加効果情報を取得
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">生産方式</param>
        /// <param name="effectID">追加効果</param>
        public static WareEffect? Get(string wareID, string method, string effectID)
        {
            // ウェアIDで絞り込み
            if (_WareEffects.TryGetValue(wareID, out var methods))
            {
                Dictionary<string, WareEffect>? effects;

                // 生産方式で絞り込み
                if (!methods.TryGetValue(method, out effects))
                {
                    // デフォルトの生産方式で取得
                    if (!methods.TryGetValue("default", out effects))
                    {
                        // 生産方式取得失敗
                        return null;
                    }
                }

                return effects!.TryGetValue(effectID, out var effect) ? effect : null;
            }
            else
            {
                // 無効なウェアIDの場合は例外を投げる
                throw new ArgumentException($"Invalid WareID({wareID}).", nameof(wareID));
            }
        }
    }
}
