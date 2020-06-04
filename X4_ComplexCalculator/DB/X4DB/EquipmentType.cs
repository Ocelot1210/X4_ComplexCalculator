using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 装備種別管理用クラス
    /// </summary>
    public class EquipmentType
    {
        #region スタティックメンバ
        /// <summary>
        /// 装備種別一覧
        /// </summary>
        private readonly static Dictionary<string, EquipmentType> _EquipmentTypes = new Dictionary<string, EquipmentType>();
        #endregion

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
        /// <param name="equipmentTypeID"></param>
        /// <param name="name"></param>
        private EquipmentType(string equipmentTypeID, string name)
        {
            EquipmentTypeID = equipmentTypeID;
            Name = name;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _EquipmentTypes.Clear();
            DBConnection.X4DB.ExecQuery($"SELECT EquipmentTypeID, Name FROM EquipmentType", (dr, args) =>
            {
                var id = (string)dr["EquipmentTypeID"];
                var name = (string)dr["Name"];

                _EquipmentTypes.Add(id, new EquipmentType(id, name));
            });
        }


        /// <summary>
        /// 装備種別IDに対応する装備種別を取得する
        /// </summary>
        /// <param name="equipmentTypeID">装備種別ID</param>
        /// <returns>装備種別</returns>
        public static EquipmentType Get(string equipmentTypeID) => _EquipmentTypes[equipmentTypeID];


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj)
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
