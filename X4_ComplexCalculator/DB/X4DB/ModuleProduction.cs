using System.Collections.Generic;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュール建造方式管理クラス
    /// </summary>
    public class ModuleProduction
    {
        /// <summary>
        /// 建造方式
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// 建造時間
        /// </summary>
        public double Time { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="method">建造方式</param>
        /// <param name="time">建造時間</param>
        public ModuleProduction(string method, double time)
        {
            Method = method;
            Time = time;
        }

        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Method.GetHashCode();
        }
    }
}
