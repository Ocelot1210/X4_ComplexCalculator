using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// ウェアの生産量と生産時間情報用クラス
    /// </summary>
    public class WareProduction : IWareProduction
    {
        #region IWareProduction
        /// <inheritdoc/>
        public string WareID { get; }


        /// <inheritdoc/>
        public string Method { get; }


        /// <inheritdoc/>
        public string Name { get; }


        /// <inheritdoc/>
        public long Amount { get; }


        /// <inheritdoc/>
        public double Time { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="method">生産方式</param>
        /// <param name="Name">名称</param>
        /// <param name="amount">生産量</param>
        /// <param name="time">生産時間</param>
        public WareProduction(string wareID, string method, string name, long amount, double time)
        {
            WareID = wareID;
            Method = method;
            Name = name;
            Amount = amount;
            Time = time;
        }


        public override string ToString() => Name;
    }
}
