using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 艦船種別情報用クラス
    /// </summary>
    public class ShipType : IShipType
    {
        #region プロパティ
        /// <inheritdoc/>
        public string ShipTypeID { get; }


        /// <inheritdoc/>
        public string Name { get; }


        /// <inheritdoc/>
        public string Description { get; }
        #endregion


        /// <summary>
        /// 艦船種別
        /// </summary>
        /// <param name="shipTypeID">艦船種別ID</param>
        /// <param name="name">名称</param>
        /// <param name="description">説明文</param>
        public ShipType(string shipTypeID, string name, string description)
        {
            ShipTypeID = shipTypeID;
            Name = name;
            Description = description;
        }
    }
}
