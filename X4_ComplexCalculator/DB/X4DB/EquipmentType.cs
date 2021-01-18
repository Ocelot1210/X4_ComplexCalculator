using System;
using System.Collections.Generic;

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
        private readonly static Dictionary<string, EquipmentType> _EquipmentTypes = new();
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
            X4Database.Instance.ExecQuery($"SELECT EquipmentTypeID, Name FROM EquipmentType", (dr, args) =>
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
        public override bool Equals(object? obj) => obj is EquipmentType tgt && tgt.EquipmentTypeID == EquipmentTypeID;


        public static bool operator ==(EquipmentType e1, EquipmentType e2)
            => e1.EquipmentTypeID == e2.EquipmentTypeID;

        public static bool operator !=(EquipmentType e1, EquipmentType e2)
            => e1.EquipmentTypeID != e2.EquipmentTypeID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(EquipmentTypeID);
    }
}
