using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB.Interfaces
{
    /// <summary>
    /// 装備情報用インターフェース
    /// </summary>
    public interface IEquipment : IWare, IMacro
    {
        #region プロパティ
        /// <summary>
        /// 装備種別
        /// </summary>
        public EquipmentType EquipmentType { get; }


        /// <summary>
        /// 船体値
        /// </summary>
        public long Hull { get; }


        /// <summary>
        /// 船体値が統合されているか
        /// </summary>
        public bool HullIntegrated { get; }


        /// <summary>
        /// Mk
        /// </summary>
        public long Mk { get; }


        /// <summary>
        /// 製造種族
        /// </summary>
        public Race? MakerRace { get; }


        /// <summary>
        /// タグ情報
        /// </summary>
        public HashSet<string> EquipmentTags { get; }


        /// <summary>
        /// サイズ
        /// </summary>
        public X4Size? Size { get; }
        #endregion
    }
}
