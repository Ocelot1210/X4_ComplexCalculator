﻿using System;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 装備品管理用クラス
    /// </summary>
    public class Equipment : IComparable
    {
        #region プロパティ
        /// <summary>
        /// 装備品ID
        /// </summary>
        public string EquipmentID { get; }

        /// <summary>
        /// 装備種別
        /// </summary>
        public EquipmentType EquipmentType { get; }

        /// <summary>
        /// 装備の大きさ
        /// </summary>
        public Size Size { get; }

        /// <summary>
        /// 装備名称
        /// </summary>
        public string Name { get; }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備品ID</param>
        public Equipment(string equipmentID)
        {
            EquipmentID = equipmentID;
            string name = "";
            EquipmentType? equipmentType = null;
            Size?          size          = null;

            DBConnection.X4DB.ExecQuery(
                $"SELECT * FROM Equipment WHERE EquipmentID = '{EquipmentID}'",
                (SQLiteDataReader dr, object[] args) =>
                    {
                        equipmentType = new EquipmentType((string)dr["EquipmentTypeID"]);
                        size = new Size((string)dr["SizeID"]);
                        name = (string)dr["Name"];
                    });

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Invalid equipment id.", nameof(equipmentID));
            }

            Name = name;
            Size = size ?? throw new InvalidOperationException($"{nameof(size)} should not be null.");
            EquipmentType = equipmentType ?? throw new InvalidOperationException($"{nameof(equipmentType)} should not be null.");
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
            return EquipmentID.CompareTo(obj is Equipment);
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
            return obj is Equipment tgt && tgt.EquipmentID == EquipmentID;
        }


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode()
        {
            return EquipmentID.GetHashCode();
        }
    }
}
