using System;
using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// モジュールの装備管理用クラス
    /// </summary>
    public class ModuleEquipment
    {
        #region "プロパティ"
        /// <summary>
        /// タレット情報
        /// </summary>
        public ModuleEquipmentManager Turret { get; set; }


        /// <summary>
        /// シールド情報
        /// </summary>
        public ModuleEquipmentManager Shield { get; set; }


        /// <summary>
        /// 装備を持っているか
        /// </summary>
        public bool CanEquipped => Turret.CanEquipped | Shield.CanEquipped;
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moduleID">モジュールID</param>
        public ModuleEquipment(string moduleID)
        {
            Turret = new ModuleEquipmentManager(moduleID, "Turret");
            Shield = new ModuleEquipmentManager(moduleID, "Shield");
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="moduleEquipment"></param>
        public ModuleEquipment(ModuleEquipment moduleEquipment)
        {
            Turret = new ModuleEquipmentManager(moduleEquipment.Turret);
            Shield = new ModuleEquipmentManager(moduleEquipment.Shield);
        }


        /// <summary>
        /// 全装備を列挙する
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Equipment> GetAllEquipment()
        {
            foreach(var turret in Turret.AllEquipments)
            {
                yield return turret;
            }
            foreach(var shiled in Shield.AllEquipments)
            {
                yield return shiled;
            }
        }


        public override bool Equals(object? obj)
        {
            return obj is ModuleEquipment equipment &&
                   EqualityComparer<ModuleEquipmentManager>.Default.Equals(Turret, equipment.Turret) &&
                   EqualityComparer<ModuleEquipmentManager>.Default.Equals(Shield, equipment.Shield);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Turret, Shield);
        }
    }
}
