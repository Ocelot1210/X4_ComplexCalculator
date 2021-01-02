using System;
using System.Collections.Generic;
using System.Text;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// エンジン情報
    /// </summary>
    public class Engine : Equipment
    {
        #region プロパティ
        /// <summary>
        /// 前方推進力
        /// </summary>
        public long ForwardThrust { get; }


        /// <summary>
        /// 後方推進力
        /// </summary>
        public long ReverseThrust { get; }


        /// <summary>
        /// ブースト推進力
        /// </summary>
        public long BoostThrust { get; }


        /// <summary>
        /// ブースト持続時間
        /// </summary>
        public double BoostDuration { get; }


        /// <summary>
        /// ブースト解除時間
        /// </summary>
        public double BoostReleaseTime { get; }


        /// <summary>
        /// トラベル推進力
        /// </summary>
        public long TravelThrust { get; }


        /// <summary>
        /// トラベル解除時間
        /// </summary>
        public double TravelReleaseTime { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <param name="macro">マクロ名</param>
        /// <param name="name">装備名称</param>
        /// <param name="equipmentTypeID">装備種別ID</param>
        /// <param name="sizeID">装備サイズ</param>
        /// <param name="hull">船体値</param>
        /// <param name="hullIntegrated">船体値が統合されているか</param>
        /// <param name="mk">MK</param>
        /// <param name="makerRace">製造種族</param>
        /// <param name="description">説明文</param>
        internal Engine(
            string equipmentID,
            string macro,
            string name,
            string equipmentTypeID,
            string sizeID,
            long hull,
            bool hullIntegrated,
            long mk,
            string? makerRace,
            string description
        ) : base(equipmentID, macro, name, equipmentTypeID, sizeID, hull, hullIntegrated, mk, makerRace, description)
        {
            const string sql = "SELECT ForwardThrust ReverseThrust, BoostThrust, BoostDuration, BoostReleaseTime, TravelThrust, TravelReleaseTime FROM Engine WHERE EquipmentID = :EquipmentID";

            (
                ForwardThrust,
                ReverseThrust,
                BoostThrust,
                BoostDuration,
                BoostReleaseTime,
                TravelThrust,
                TravelReleaseTime
            ) = X4Database.Instance.QuerySingle<(long, long, long, double, double, long, double)> (sql, new { EquipmentID = equipmentID });
        }
    }
}
