namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 艦船のハンガー情報を管理するクラス
    /// </summary>
    public class ShipHanger
    {
        #region プロパティ
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ShipID { get; }


        /// <summary>
        /// 発着パッドのサイズ
        /// </summary>
        public X4Size Size { get; }


        /// <summary>
        /// 発着パッド数
        /// </summary>
        public long Count { get; }


        /// <summary>
        /// 機体格納数
        /// </summary>
        public long Capacity { get; }
        #endregion


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
            Size = X4Database.Instance.X4Size.Get(sizeID);
            Count = count;
            Capacity = capacity;
        }
    }
}
