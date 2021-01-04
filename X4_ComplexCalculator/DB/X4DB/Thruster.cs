using System;
using System.Collections.Generic;
using System.Text;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// スラスター情報を管理するクラス
    /// </summary>
    public class Thruster : Equipment
    {
        #region プロパティ
        /// <summary>
        /// 推進(水平・垂直)？
        /// </summary>
        public double ThrustStrafe { get; }


        /// <summary>
        /// 推進(ピッチ)
        /// </summary>
        public double ThrustPitch { get; }


        /// <summary>
        /// 推進(ヨー)
        /// </summary>
        public double ThrustYaw { get; }


        /// <summary>
        /// 推進(ロール)
        /// </summary>
        public double ThrustRoll { get; }


        /// <summary>
        /// 角度(ロール)？
        /// </summary>
        public double AngularRoll { get; }


        /// <summary>
        /// 角度(ピッチ)？
        /// </summary>
        public double AngularPitch { get; }
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
        internal Thruster(
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
            const string sql = "SELECT ThrustStrafe, ThrustPitch, ThrustYaw, ThrustRoll, AngularRoll, AngularPitch FROM Thruster WHERE EquipmentID = :EquipmentID";

            (
                ThrustStrafe,
                ThrustPitch,
                ThrustYaw,
                ThrustRoll,
                AngularRoll,
                AngularPitch
            ) = X4Database.Instance.QuerySingle<(double, double, double, double, double, double)>(sql, new { EquipmentID = equipmentID });
        }
    }
}
