using System;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 装備種別管理用クラス
    /// </summary>
    public class EquipmentType : IComparable
    {
        /// <summary>
        /// 装備種別ID
        /// </summary>
        public string EquipmentTypeID { private set; get; }

        /// <summary>
        /// 装備種別名
        /// </summary>
        public string Name { private set; get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="id">装備種別ID</param>
        public EquipmentType(string id)
        {
            EquipmentTypeID = id;
            DBConnection.X4DB.ExecQuery($"SELECT * FROM EquipmentType WHERE EquipmentTypeID = '{EquipmentTypeID}'", (SQLiteDataReader dr, object[] args) => { Name = dr["Name"].ToString(); });
        }


        /// <summary>
        /// オブジェクト比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            return EquipmentTypeID.CompareTo(obj is EquipmentType);
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
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
