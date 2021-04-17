using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// 1サイクルのウェア生産に必要なウェア情報用クラス
    /// </summary>
    public class WareResource : IWareResource
    {
        #region プロパティ
        /// <inheritdoc/>
        public string WareID { get; }


        /// <inheritdoc/>
        public string Method { get; }


        /// <inheritdoc/>
        public string NeedWareID { get; }


        /// <inheritdoc/>
        public long Amount { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">生産方式</param>
        /// <param name="needWareID">必要ウェアID</param>
        /// <param name="amount">必要量</param>
        public WareResource(string wareID, string method, string needWareID, long amount)
        {
            WareID = wareID;
            Method = method;
            NeedWareID = needWareID;
            Amount = amount;
        }
    }
}
