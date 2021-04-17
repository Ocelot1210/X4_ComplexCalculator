using System.Collections.Generic;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// 製品の計算結果を表すクラス
    /// </summary>
    class CalcResult
    {
        #region プロパティ
        /// <summary>
        /// 生産/消費ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// ウェア生産/消費量
        /// </summary>
        public long WareAmount { get; }


        /// <summary>
        /// ウェア生産方式
        /// </summary>
        public string Method { get; }


        /// <summary>
        /// 生産/消費ウェアを生産/消費するモジュール
        /// </summary>
        public IX4Module Module { get; }
        

        /// <summary>
        /// 生産/消費ウェアを生産/消費するモジュールの数
        /// </summary>
        public long ModuleCount { get; }


        /// <summary>
        /// ウェア生産の追加効果
        /// </summary>
        public IReadOnlyDictionary<string, IWareEffect>? Efficiency { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">生産/消費ウェアID</param>
        /// <param name="wareAmount">ウェア生産/消費量</param>
        /// <param name="method">ウェア生産方式</param>
        /// <param name="module">生産/消費ウェアを生産/消費するモジュール</param>
        /// <param name="moduleCount">生産/消費ウェアを生産/消費するモジュールの数</param>
        /// <param name="efficiency">ウェア生産の追加効果</param>
        public CalcResult(string wareID, long wareAmount, string method, IX4Module module, long moduleCount, IReadOnlyDictionary<string, IWareEffect>? efficiency = null)
        {
            WareID = wareID;
            WareAmount = wareAmount;
            Method = method;
            Module = module;
            ModuleCount = moduleCount;
            Efficiency = efficiency;
        }
    }
}
