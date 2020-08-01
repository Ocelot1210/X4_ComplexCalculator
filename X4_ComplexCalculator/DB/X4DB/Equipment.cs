using System;
using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 装備品管理用クラス
    /// </summary>
    public class Equipment
    {
        #region スタティックメンバ
        /// <summary>
        /// 装備一覧
        /// </summary>
        private readonly static Dictionary<string, Equipment> _Equipments = new Dictionary<string, Equipment>();
        #endregion


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
        /// <param name="equipmentID"></param>
        /// <param name="name"></param>
        /// <param name="equipmentType"></param>
        /// <param name="size"></param>
        private Equipment(string equipmentID, string name, EquipmentType equipmentType, Size size)
        {
            EquipmentID = equipmentID;
            Name = name;
            EquipmentType = equipmentType;
            Size = size;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            _Equipments.Clear();
            DBConnection.X4DB.ExecQuery("SELECT EquipmentID, EquipmentTypeID, SizeID, Name FROM Equipment", (dr, args) =>
            {
                var id = (string)dr["EquipmentID"];
                var type = (string)dr["EquipmentTypeID"];
                var size = (string)dr["SizeID"];
                var name = (string)dr["Name"];

                _Equipments.Add(id, new Equipment(id, name, EquipmentType.Get(type), Size.Get(size)));
            });
        }


        /// <summary>
        /// 装備IDに対応する装備を取得する
        /// </summary>
        /// <param name="equipmentID">装備ID</param>
        /// <returns>装備</returns>
        public static Equipment? Get(string equipmentID) =>
            _Equipments.TryGetValue(equipmentID, out var equipment) ? equipment : null;


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is Equipment tgt && EquipmentID == tgt.EquipmentID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(EquipmentID);
    }
}
