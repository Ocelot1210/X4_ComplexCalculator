using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    /// <summary>
    /// シールド情報
    /// </summary>
    public partial class Shield : IShield
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipment"></param>
        /// <param name="capacity">最大シールド容量</param>
        /// <param name="rechargeRate">再充電率</param>
        /// <param name="rechargeDelay">再充電遅延</param>
        public Shield(IEquipment equipment, long capacity, long rechargeRate, double rechargeDelay)
        {
            ID = equipment.ID;
            Name = equipment.Name;
            WareGroup = equipment.WareGroup;
            TransportType = equipment.TransportType;
            Description = equipment.Description;
            Volume = equipment.Volume;
            MinPrice = equipment.MinPrice;
            AvgPrice = equipment.AvgPrice;
            MaxPrice = equipment.MaxPrice;
            Owners = equipment.Owners;
            Productions = equipment.Productions;
            Resources = equipment.Resources;
            Tags = equipment.Tags;
            WareEffects = equipment.WareEffects;

            MacroName = equipment.MacroName;
            EquipmentType = equipment.EquipmentType;
            Hull = equipment.Hull;
            HullIntegrated = equipment.HullIntegrated;
            Mk = equipment.Mk;
            MakerRace = equipment.MakerRace;
            EquipmentTags = equipment.EquipmentTags;
            Size = equipment.Size;

            Capacity = capacity;
            RechargeRate = rechargeRate;
            RechargeDelay = rechargeDelay;
        }
    }
}
