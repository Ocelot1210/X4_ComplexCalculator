using System;
using System.Collections.Generic;
using System.Text;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// シールド情報
    /// </summary>
    public class Shield : Equipment
    {
        #region プロパティ
        /// <summary>
        /// 最大シールド容量
        /// </summary>
        public long Capacity { get; }


        /// <summary>
        /// 再充電率
        /// </summary>
        public long RechargeRate { get; }


        /// <summary>
        /// 再充電遅延
        /// </summary>
        public double RechargeDelay { get; }
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
        internal Shield(
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
            const string sql = "SELECT Capacity RechargeRate, RechargeDelay FROM Shield WHERE EquipmentID = :EquipmentID";

            (
                Capacity,
                RechargeRate,
                RechargeDelay
            ) = X4Database.Instance.QuerySingle<(long, long, double)>(sql, new { EquipmentID = equipmentID });
        }
    }
}
