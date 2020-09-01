using System;
using System.Collections.Generic;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュール建造方式管理クラス
    /// </summary>
    public class ModuleProduction
    {
        #region スタティックメンバ
        /// <summary>
        /// モジュール建造方式一覧
        /// </summary>
        private static Dictionary<string, ModuleProduction[]> _ModuleProductions = new Dictionary<string, ModuleProduction[]>();
        #endregion

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
        private ModuleProduction(string method, double time)
        {
            Method = method;
            Time = time;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _ModuleProductions.Clear();

            var dict = new Dictionary<string, List<ModuleProduction>>();
            X4Database.Instance.ExecQuery($"SELECT ModuleID, Method, Time FROM ModuleProduction", (dr, _) =>
            {
                var id = (string)dr["ModuleID"];
                if (!dict.ContainsKey(id))
                {
                    dict.Add(id, new List<ModuleProduction>());
                }

                dict[id].Add(new ModuleProduction((string)dr["Method"], (double)dr["Time"]));
            });

            _ModuleProductions = dict.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }


        /// <summary>
        /// モジュールIDに対応するモジュール建造方式一覧を取得
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <returns>モジュール建造方式一覧</returns>
        public static ModuleProduction[] Get(string moduleID) => _ModuleProductions[moduleID];



        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => HashCode.Combine(Method);
    }
}
