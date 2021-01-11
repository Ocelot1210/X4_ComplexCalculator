using System;
using System.Collections.Generic;
using System.Text;
using X4_ComplexCalculator.DB.X4DB;
using System.Linq;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Ships
{
    /// <summary>
    /// サイズ単位のエンジンを管理するクラス
    /// </summary>
    class EngineManager
    {
        #region プロパティ
        /// <summary>
        /// 最高速度のエンジン
        /// </summary>
        public Engine MaxForwardEngine { get; }


        /// <summary>
        /// 最高後退速度のエンジン
        /// </summary>
        public Engine MaxReverseSpeedEngine { get; }


        /// <summary>
        /// 最高ブースト速度のエンジン
        /// </summary>
        public Engine MaxBoostSpeedEngine { get; }


        /// <summary>
        /// 最高トラベル速度のエンジン
        /// </summary>
        public Engine MaxTravelSpeedEngine { get; }


        /// <summary>
        /// 最高加速のエンジン
        /// </summary>
        public Engine MaxAccelerateEngine { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="engines">エンジン一覧</param>
        public EngineManager(IEnumerable<Engine> engines)
        {
#if DEBUG
            // サイズ違いのエンジンが混じってたら例外を投げる
            if (1 < engines.GroupBy(x => x.Size.SizeID).Count())
            {
                throw new ArgumentException("The size of the engine is not unified.", nameof(engines));
            }
#endif

            MaxForwardEngine      = engines.OrderByDescending(x => x.ForwardThrust).First();
            MaxReverseSpeedEngine = engines.OrderByDescending(x => x.ReverseThrust).First();
            MaxBoostSpeedEngine   = engines.OrderByDescending(x => x.BoostThrust).First();
            MaxTravelSpeedEngine  = engines.OrderByDescending(x => x.TravelThrust).First();
            MaxAccelerateEngine   = MaxForwardEngine;       // 前方推力が最高のエンジンが最大の加速を生む
        }


        /// <summary>
        /// コンストラクタ(デフォルトのロードアウトがある艦船専用)
        /// </summary>
        /// <param name="engine">エンジン</param>
        public EngineManager(Engine engine)
        {
            MaxForwardEngine =
            MaxReverseSpeedEngine =
            MaxBoostSpeedEngine =
            MaxTravelSpeedEngine =
            MaxAccelerateEngine = engine;
        }
    }
}
