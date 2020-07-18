using System.ComponentModel;

namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// 製品詳細表示用インターフェイス
    /// </summary>
    public interface IProductDetailsListItem : INotifyPropertyChanged
    {
        /// <summary>
        /// モジュールID
        /// </summary>
        string ModuleID { get; }


        /// <summary>
        /// モジュール名
        /// </summary>
        string ModuleName { get; }


        /// <summary>
        /// モジュール数
        /// </summary>
        long ModuleCount { get; set; }


        /// <summary>
        /// 生産性
        /// </summary>
        double Efficiency { get; }


        /// <summary>
        /// 生産量
        /// </summary>
        long Amount { get; }


        /// <summary>
        /// 生産性を設定
        /// </summary>
        /// <param name="effectID">効果ID</param>
        /// <param name="value">設定値</param>
        void SetEfficiency(string effectID, double value);
    }
}
