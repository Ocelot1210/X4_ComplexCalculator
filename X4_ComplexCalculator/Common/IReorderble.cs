namespace X4_ComplexCalculator.Common
{
    /// <summary>
    /// 順番を入れ替え可能なコレクションの要素に対するインターフェイス
    /// </summary>
    interface IReorderble
    {
        /// <summary>
        /// 順番入れ替え対象か
        /// </summary>
        public bool IsReorderTarget { get; set; }
    }
}
