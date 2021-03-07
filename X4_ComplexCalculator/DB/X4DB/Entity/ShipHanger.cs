using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// 艦船のハンガー情報用クラス
    /// </summary>
    public class ShipHanger : IShipHanger
    {
        #region IShipHanger
        /// <inheritdoc/>
        public string ShipID { get; }


        /// <inheritdoc/>
        public IX4Size Size { get; }


        /// <inheritdoc/>
        public long Count { get; }


        /// <inheritdoc/>
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
