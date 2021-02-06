using System;
using System.Collections.Generic;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// 装備品管理用クラス
    /// </summary>
    public partial class Equipment : IEquipment
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ware">ウェア情報</param>
        /// <param name="macro">マクロ名</param>
        /// <param name="equipmentType">装備種別</param>
        /// <param name="hull">船体値</param>
        /// <param name="hullIntegrated">船体値が統合されているか</param>
        /// <param name="mk">Mk</param>
        /// <param name="makerRace">製造種族</param>
        /// <param name="equipmentTags">タグ情報</param>
        /// <param name="size">サイズ</param>
        public Equipment(
            IWare ware,
            string macro,
            EquipmentType equipmentType,
            long hull,
            bool hullIntegrated,
            long mk,
            Race? makerRace,
            HashSet<string> equipmentTags,
            X4Size? size
        )
        {
            ID = ware.ID;
            Name = ware.Name;
            WareGroup = ware.WareGroup;
            TransportType = ware.TransportType;
            Description = ware.Description;
            Volume = ware.Volume;
            MinPrice = ware.MinPrice;
            AvgPrice = ware.AvgPrice;
            MaxPrice = ware.MaxPrice;
            Owners = ware.Owners;
            Productions = ware.Productions;
            Resources = ware.Resources;
            Tags = ware.Tags;
            WareEffects = ware.WareEffects;

            Macro = macro;
            EquipmentType = equipmentType;
            Hull = hull;
            HullIntegrated = hullIntegrated;
            Mk = mk;
            MakerRace = makerRace;
            EquipmentTags = equipmentTags;
            Size = size;
        }


        /// <summary>
        /// 比較
        /// </summary>
        /// <param name="obj">比較対象</param>
        /// <returns></returns>
        public override bool Equals(object? obj) => obj is IWare other && ID == other.ID;


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(ID);
    }
}
