using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 1サイクルのウェア生産に必要なウェア情報
    /// </summary>
    public class WareResource
    {
        #region スタティックメンバ
        /// <summary>
        /// 1サイクルのウェア生産に必要なウェア情報一覧
        /// </summary>
        private static Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<WareResource>>> _WareResources = new();


        /// <summary>
        /// ダミー用1サイクルのウェア生産に必要なウェア情報一覧
        /// </summary>
        private readonly static Dictionary<string, IReadOnlyList<WareResource>> _DummyResources 
            = new Dictionary<string, IReadOnlyList<WareResource>>();
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
        /// 必要ウェアID
        /// </summary>
        public string NeedWareID { get; }


        /// <summary>
        /// 必要量
        /// </summary>
        public long Amount { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">生産方式</param>
        /// <param name="needWareID">必要ウェアID</param>
        /// <param name="amount">必要量</param>
        private WareResource(string wareID, string method, string needWareID, long amount)
        {
            WareID = wareID;
            Method = method;
            NeedWareID = needWareID;
            Amount = amount;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            const string sql = "SELECT WareID, Method, NeedWareID, Amount FROM WareResource";

            _WareResources = X4Database.Instance.Query<WareResource>(sql)
                .GroupBy(x => x.WareID)
                .ToDictionary(
                    x => x.Key,
                    x => x.GroupBy(y => y.Method).ToDictionary(y => y.Key, y => y.ToArray() as IReadOnlyList<WareResource>) as IReadOnlyDictionary<string, IReadOnlyList<WareResource>>
                );
        }


        /// <summary>
        /// ウェアIDに対応する必要ウェア情報を取得する
        /// </summary>
        /// <param name="id">ウェアID</param>
        /// <returns>必要ウェア情報</returns>
        public static IReadOnlyDictionary<string, IReadOnlyList<WareResource>> Get(string id)
            => _WareResources.TryGetValue(id, out var ret) ? ret : _DummyResources;
    }
}
