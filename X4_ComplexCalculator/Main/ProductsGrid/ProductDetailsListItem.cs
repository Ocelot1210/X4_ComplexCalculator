using System.Data.SQLite;
using X4_ComplexCalculator.Common;
using X4_ComplexCalculator.DB;

namespace X4_ComplexCalculator.Main.ProductsGrid
{
    /// <summary>
    /// ドロップダウンで表示するListViewのアイテム(製品用)
    /// </summary>
    public class ProductDetailsListItem : INotifyPropertyChangedBace
    {
        #region メンバ
        /// <summary>
        /// 製品数(モジュール追加用)
        /// </summary>
        private readonly long _Amount;

        /// <summary>
        /// 生産性
        /// </summary>
        private double _Efficiency;
        #endregion

        /// <summary>
        /// モジュールID
        /// </summary>
        public string ModuleID { get; }


        /// <summary>
        /// モジュール名
        /// </summary>
        public string ModuleName { get; }


        /// <summary>
        /// モジュール数
        /// </summary>
        public long ModuleCount { get; private set; }


        /// <summary>
        /// 生産性(効率)
        /// </summary>
        public string Efficiency { get; private set; }


        /// <summary>
        /// 製品数
        /// </summary>
        public long Amount => _Amount * ModuleCount;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        /// <param name="moduleCount">モジュール数</param>
        /// <param name="efficiency">効率</param>
        /// <param name="amount">製品数</param>
        public ProductDetailsListItem(string moduleID, long moduleCount, double efficiency, long amount)
        {
            ModuleID = moduleID;
            ModuleCount = moduleCount;
            _Amount = amount;
            _Efficiency = efficiency;

            Efficiency = (efficiency < 0) ? "-" : $"{(int)(efficiency * 100)}%";


            var moduleName = "";
            DBConnection.X4DB.ExecQuery($"SELECT Name FROM Module WHERE ModuleID = '{moduleID}'", (SQLiteDataReader dr, object[] args) => { moduleName = (string)dr["Name"]; });
            ModuleName = moduleName;
        }

        /// <summary>
        /// モジュールが増えたことにする
        /// </summary>
        /// <param name="count">増分量</param>
        public void Incriment(long count)
        {
            ModuleCount += count;
        }
    }
}
