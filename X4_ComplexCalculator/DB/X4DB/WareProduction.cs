using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// ウェアの生産量と生産時間を管理するクラス
    /// </summary>
    public class WareProduction
    {
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
    }
}
