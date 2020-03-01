using System;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 装備品管理用クラス
    /// </summary>
    public class Equipment : IComparable
    {
        /// <summary>
        /// 装備品ID
        /// </summary>
        public string EquipmentID { private set; get; }

        /// <summary>
        /// 装備種別
        /// </summary>
        public EquipmentType EquipmentType { private set; get; }

        /// <summary>
        /// 装備の大きさ
        /// </summary>
        public Size Size { private set; get; }

        /// <summary>
        /// 装備名称
        /// </summary>
        public string Name { private set; get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipmentID">装備品ID</param>
        public Equipment(string equipmentID)
        {
            EquipmentID = equipmentID;

            DBConnection.X4DB.ExecQuery(
                $"SELECT * FROM Equipment WHERE EquipmentID = '{EquipmentID}'",
                (SQLiteDataReader dr, object[] args) =>
                    {
                        EquipmentType = new EquipmentType(dr["EquipmentTypeID"].ToString());
                        Size = new Size(dr["SizeID"].ToString());
                        Name = dr["Name"].ToString();
                    });
        }

        /// <summary>
        /// オブジェクト比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            return EquipmentID.CompareTo(obj is Equipment);
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
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
