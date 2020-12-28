using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// ウェアの生産量と生産時間を管理するクラス
    /// </summary>
    public class WareProduction
    {
        #region スタティックメンバ
        /// <summary>
        /// ウェアの生産量と生産時間情報の一覧
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, WareProduction>> _WareProductions = new();
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
        /// 名称
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 生産量
        /// </summary>
        public long Amount { get; }


        /// <summary>
        /// 生産時間
        /// </summary>
        public double Time { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">生産方式</param>
        /// <param name="Name"></param>
        /// <param name="amount"></param>
        /// <param name="time"></param>
        private WareProduction(string wareID, string method, string name, long amount, double time)
        {
            WareID = wareID;
            Method = method;
            Name = name;
            Amount = amount;
            Time = time;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _WareProductions.Clear();

            const string sql = "SELECT WareID, Method, Name, Amount, Time FROM WareProduction";
            foreach (var item in X4Database.Instance.Query<WareProduction>(sql))
            {
                if (!_WareProductions.ContainsKey(item.WareID))
                {
                    _WareProductions.Add(item.WareID, new Dictionary<string, WareProduction>());
                }

                _WareProductions[item.WareID].Add(item.Method, item);
            }
        }


        /// <summary>
        /// ウェアIDと生産方式に対応する値を取得する
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">生産方式</param>
        /// <returns></returns>
        public static WareProduction? Get(string wareID, string method)
        {
            // ウェアIDで絞り込み
            if (_WareProductions.TryGetValue(wareID, out var methods))
            {
                // 生産方式で絞り込み
                if (methods.TryGetValue(method, out var production))
                {
                    return production;
                }

                // デフォルトの生産方式で絞り込み
                if (methods.TryGetValue("default", out var defaultProduction))
                {
                    return defaultProduction;
                }
            }

            return null;
        }
    }
}
