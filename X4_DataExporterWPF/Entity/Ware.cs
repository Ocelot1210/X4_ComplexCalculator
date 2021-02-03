namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// ウェア
    /// </summary>
    public class Ware
    {
        #region プロパティ
        /// <summary>
        /// ウェアID
        /// </summary>
        public string WareID { get; }


        /// <summary>
        /// ウェア種別ID
        /// </summary>
        public string? WareGroupID { get; }


        /// <summary>
        /// カーゴ種別ID
        /// </summary>
        public string? TransportTypeID { get; }


        /// <summary>
        /// ウェア名称
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 説明
        /// </summary>
        public string Description { get; }


        /// <summary>
        /// 大きさ
        /// </summary>
        public int Volume { get; }


        /// <summary>
        /// 最低価格
        /// </summary>
        public int MinPrice { get; }


        /// <summary>
        /// 平均価格
        /// </summary>
        public int AvgPrice { get; }


        /// <summary>
        /// 最高価格
        /// </summary>
        public int MaxPrice { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="wareID">ウェアID</param>
        /// <param name="wareGroupID">ウェア種別ID</param>
        /// <param name="transportTypeID">カーゴ種別ID</param>
        /// <param name="name">ウェア名称</param>
        /// <param name="description">説明</param>
        /// <param name="volume">大きさ</param>
        /// <param name="minPrice">MinPrice</param>
        /// <param name="avgPrice">平均価格</param>
        /// <param name="maxPrice">MaxPrice</param>
        public Ware(
            string wareID, string? wareGroupID, string? transportTypeID,
            string name, string description,
            int volume, int minPrice, int avgPrice, int maxPrice
        )
        {
            WareID = wareID;
            WareGroupID = wareGroupID;
            TransportTypeID = transportTypeID;
            Name = name;
            Description = description;
            Volume = volume;
            MinPrice = minPrice;
            AvgPrice = avgPrice;
            MaxPrice = maxPrice;
        }
    }
}
