namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 艦船種別
    /// </summary>
    public class ShipType
    {
        #region プロパティ
        /// <summary>
        /// 艦船種別ID
        /// </summary>
        public string ShipTypeID { get; }


        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 説明文
        /// </summary>
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
