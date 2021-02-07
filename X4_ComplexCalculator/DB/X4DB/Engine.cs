using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// エンジン情報
    /// </summary>
    public partial class Engine : IEngine
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipment">装備情報</param>
        /// <param name="thrust">推進力情報</param>
        /// <param name="boostDuration">ブースト持続時間</param>
        /// <param name="boostReleaseTime">ブースト解除時間</param>
        /// <param name="travelReleaseTime">トラベル解除時間</param>
        public Engine(
            IEquipment equipment,
            EngineThrust thrust,
            double boostDuration,
            double boostReleaseTime,
            double travelReleaseTime
        )
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

            Thrust = thrust;
            BoostDuration = boostDuration;
            BoostReleaseTime = boostReleaseTime;
            TravelReleaseTime = travelReleaseTime;
        }
    }
}
