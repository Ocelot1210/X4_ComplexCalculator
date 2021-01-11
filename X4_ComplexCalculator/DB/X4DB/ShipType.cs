using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 艦船種別
    /// </summary>
    public class ShipType
    {
        #region スタティックメンバ
        /// <summary>
        /// 艦船種別一覧
        /// </summary>
        private static readonly Dictionary<string, ShipType> _ShipTypes = new();
        #endregion


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
        private ShipType(string shipTypeID, string name, string description)
        {
            ShipTypeID = shipTypeID;
            Name = name;
            Description = description;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _ShipTypes.Clear();

            const string sql = "SELECT ShipTypeID, Name, Description FROM ShipType";
            foreach (var shipType in X4Database.Instance.Query<ShipType>(sql))
            {
                _ShipTypes.Add(shipType.ShipTypeID, shipType);
            }
        }


        /// <summary>
        /// 艦船種別を取得する
        /// </summary>
        /// <param name="shipTypeID">艦船種別ID</param>
        /// <returns>艦船種別IDに対応する艦船種別</returns>
        public static ShipType Get(string shipTypeID) => _ShipTypes[shipTypeID];
    }
}
