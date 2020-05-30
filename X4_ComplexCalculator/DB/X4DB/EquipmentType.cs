using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 装備種別管理用クラス
    /// </summary>
    public class EquipmentType : IComparable
    {
        #region プロパティ
        /// <summary>
        /// 装備種別ID
        /// </summary>
        public string EquipmentTypeID { get; }

        /// <summary>
        /// 装備種別名
        /// </summary>
        public string Name { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentTypeID">装備種別ID</param>
        public EquipmentType(string equipmentTypeID)
        {
            EquipmentTypeID = equipmentTypeID;
            string name = "";
            DBConnection.X4DB.ExecQuery($"SELECT * FROM EquipmentType WHERE EquipmentTypeID = '{EquipmentTypeID}'", (SQLiteDataReader dr, object[] args) => { name = (string)dr["Name"]; });

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid equipment type id.", nameof(equipmentTypeID));
            }

            Name = name;
        }


        /// <summary>
        /// オブジェクト比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }
            return EquipmentTypeID.CompareTo(obj is EquipmentType);
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                throw new ArgumentException($"parameter {nameof(obj)} should not be null.", nameof(obj));
            }
            return obj is EquipmentType tgt && tgt.EquipmentTypeID == EquipmentTypeID;
        }


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode()
        {
            return EquipmentTypeID.GetHashCode();
        }
    }
}
