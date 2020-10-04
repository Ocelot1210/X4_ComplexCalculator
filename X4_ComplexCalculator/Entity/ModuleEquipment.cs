using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.DB.X4DB;

namespace X4_ComplexCalculator.Entity
{
    /// <summary>
    /// モジュールの装備管理用クラス
    /// </summary>
    public class ModuleEquipment : IEquatable<ModuleEquipment>
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
            Turret = new ModuleEquipmentCollection("turrets", module.TurretCapacity);
            Shield = new ModuleEquipmentCollection("shields", module.ShieldCapacity);
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


        /// <inheritdoc />
        public bool Equals(ModuleEquipment? other)
            => Turret.Equals(other?.Turret) && Shield.Equals(other.Shield);


        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is ModuleEquipment other && Equals(other);


        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Turret, Shield);
    }
}
