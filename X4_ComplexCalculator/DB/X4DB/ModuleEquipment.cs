using System;
using System.Collections.Generic;
using System.Linq;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュールの装備管理用クラス
    /// </summary>
    public class ModuleEquipment
    {
        #region プロパティ
        /// <summary>
        /// タレット情報
        /// </summary>
        public ModuleEquipmentManager Turret { get; }


        /// <summary>
        /// シールド情報
        /// </summary>
        public ModuleEquipmentManager Shield { get; }


        /// <summary>
        /// 装備を持っているか
        /// </summary>
        public bool CanEquipped => Turret.CanEquipped | Shield.CanEquipped;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="module">モジュール</param>
        public ModuleEquipment(Module module)
        {
            Turret = new ModuleEquipmentManager(module.TurretCapacity);
            Shield = new ModuleEquipmentManager(module.ShieldCapacity);
        }


        /// <summary>
        /// 全装備を列挙する
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Equipment> GetAllEquipment() => Turret.AllEquipments.Concat(Shield.AllEquipments);


        /// <summary>
        /// オブジェクトが同一か判定
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is ModuleEquipment equipment &&
                   EqualityComparer<ModuleEquipmentManager>.Default.Equals(Turret, equipment.Turret) &&
                   EqualityComparer<ModuleEquipmentManager>.Default.Equals(Shield, equipment.Shield);
        }


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(Turret, Shield);
    }
}
