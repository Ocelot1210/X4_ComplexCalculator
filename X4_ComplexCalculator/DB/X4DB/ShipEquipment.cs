using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 艦船の装備を管理するクラス
    /// </summary>
    public class ShipEquipment
    {
        #region スタティックメンバ
        /// <summary>
        /// 艦船IDをキーにした艦船の装備情報
        /// ＜艦船ID, ＜装備種別, ＜サイズID, 装備可能個数＞＞＞
        /// </summary>
        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, ShipEquipment>>>? _ShipEquipments;
        #endregion


        #region プロパティ
        /// <summary>
        /// 艦船ID
        /// </summary>
        public string ShipID { get; }


        /// <summary>
        /// 装備種別
        /// </summary>
        public EquipmentType EquipmentType { get; }


        /// <summary>
        /// 装備のサイズ
        /// </summary>
        public X4Size Size { get; }


        /// <summary>
        /// 装備可能な数
        /// </summary>
        public long Count { get; }
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <param name="equipmentTypeID">装備種別ID</param>
        /// <param name="sizeID">サイズID</param>
        /// <param name="count">装備可能な数</param>
        private ShipEquipment(string shipID, string equipmentTypeID, string sizeID, long count)
        {
            ShipID = shipID;
            EquipmentType = EquipmentType.Get(equipmentTypeID);
            Size = X4Size.Get(sizeID);
            Count = count;
        }


        /// <summary>
        /// 初期化
        /// </summary>
        public static void Init()
        {
            const string sql = "SELECT ShipID, EquipmentTypeID, SizeID, Count FROM ShipEquipment";
            _ShipEquipments = X4Database.Instance.Query<ShipEquipment>(sql)
                .GroupBy(x => x.ShipID)
                .ToDictionary
                (
                    x => x.Key,
                    x => x.GroupBy(y => y.EquipmentType.EquipmentTypeID)
                    .ToDictionary
                    (
                        y => y.Key,
                        y => y.ToDictionary
                        (
                            z => z.Size.SizeID,
                            z => z
                        ) as IReadOnlyDictionary<string, ShipEquipment>
                    ) as IReadOnlyDictionary<string, IReadOnlyDictionary<string, ShipEquipment>>
                );
        }


        /// <summary>
        /// 艦船IDに対応する装備情報を取得する
        /// </summary>
        /// <param name="shipID">艦船ID</param>
        /// <returns>艦船IDに対応する装備情報</returns>
        public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, ShipEquipment>> Get(string shipID)
        {
            if (_ShipEquipments is null) throw new InvalidOperationException("Not initialized!");

            return _ShipEquipments[shipID];
        }
    }
}
