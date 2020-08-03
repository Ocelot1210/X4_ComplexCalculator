namespace X4_ComplexCalculator.Main.WorkArea.UI.ProductsGrid
{
    /// <summary>
    /// 売買オプション
    /// </summary>
    public class TradeOption
    {
        #region メンバ
        /// <summary>
        /// 購入しない
        /// </summary>
        public bool NoBuy;


        /// <summary>
        /// 販売しない
        /// </summary>
        public bool NoSell;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="noBuy">購入しない</param>
        /// <param name="noSell">販売しない</param>
        public TradeOption(bool noBuy = false, bool noSell = false)
        {
            NoBuy = noBuy;
            NoSell = noSell;
        }
    }
}
