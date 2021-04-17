namespace X4_ComplexCalculator.DB.X4DB.Interfaces
{
    /// <summary>
    /// ウェア種別(グループ)情報用インターフェイス
    /// </summary>
    public interface IWareGroup
    {
        #region プロパティ
        /// <summary>
        /// ウェアグループID
        /// </summary>
        public string WareGroupID { get; }


        /// <summary>
        /// ウェアグループ名
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 階級
        /// </summary>
        public long Tier { get; }
        #endregion
    }
}
