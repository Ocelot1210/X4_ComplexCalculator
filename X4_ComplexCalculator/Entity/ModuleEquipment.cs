using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Entity
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
        public ModuleEquipmentCollection Turret { get; }


        /// <summary>
        /// シールド情報
        /// </summary>
        public ModuleEquipmentCollection Shield { get; }


        /// <summary>
        /// 装備中の全てのタレット・シールドを列挙
        /// </summary>
        public IEnumerable<Equipment> AllEquipments
            => Turret.AllEquipments.Concat(Shield.AllEquipments);


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
            Turret = new ModuleEquipmentCollection(module.TurretCapacity);
            Shield = new ModuleEquipmentCollection(module.ShieldCapacity);
        }


        /// <summary>
        /// 引数の装備品を装備する
        /// </summary>
        /// <param name="equipment">装備する装備品</param>
        public void AddEquipment(Equipment equipment, long count = 1)
        {
            if (!CanEquipped) return;
            var equipmentTypeID = equipment.EquipmentType.EquipmentTypeID;
            var collection = equipmentTypeID switch
            {
                "turrets" => Turret,
                "shields" => Shield,
                _ => throw new ArgumentException($"Invalid equipment type. ({equipmentTypeID})"),
            };
            for (var i = 0L; i < count; i++) collection.AddEquipment(equipment);
        }


        /// <summary>
        /// オブジェクトが同一か判定
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is ModuleEquipment equipment &&
                   EqualityComparer<ModuleEquipmentCollection>.Default.Equals(Turret, equipment.Turret) &&
                   EqualityComparer<ModuleEquipmentCollection>.Default.Equals(Shield, equipment.Shield);
        }


        /// <summary>
        /// ハッシュ値を取得
        /// </summary>
        /// <returns>ハッシュ値</returns>
        public override int GetHashCode() => HashCode.Combine(Turret, Shield);
    }
}
