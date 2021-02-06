using System;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 装備種別管理用クラス
    /// </summary>
    public class EquipmentType
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
        /// <param name="equipmentTypeID"></param>
        /// <param name="name"></param>
        public EquipmentType(string equipmentTypeID, string name)
        {
            EquipmentTypeID = equipmentTypeID;
            Name = name;
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is EquipmentType tgt && tgt.EquipmentTypeID == EquipmentTypeID;


        public bool Equals(EquipmentType other) => other.EquipmentTypeID == EquipmentTypeID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(EquipmentTypeID);
    }
}
