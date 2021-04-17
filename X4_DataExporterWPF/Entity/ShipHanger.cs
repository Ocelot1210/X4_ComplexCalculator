namespace X4_DataExporterWPF.Entity
{
    /// <summary>
    /// 艦船のハンガー情報
    /// </summary>
    public class ShipHanger
    {
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ShipID { get; }


        /// <summary>
        /// 発着パッドのサイズID
        /// </summary>
        public string SizeID { get; }


        /// <summary>
        /// 発着パッド数
        /// </summary>
        public long Count { get; }

        
        /// <summary>
        /// 機体格納数
        /// </summary>
        public long Capacity { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <param name="sizeID">発着パッドのサイズID</param>
        /// <param name="count">発着パッド数</param>
        /// <param name="capacity">機体格納数</param>
        public ShipHanger(string shipID, string sizeID, long count, long capacity)
        {
            ShipID = shipID;
            SizeID = sizeID;
            Count = count;
            Capacity = capacity;
        }
    }
}
