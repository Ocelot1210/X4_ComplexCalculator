using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB
{
    /// <summary>
    /// スラスター情報を管理するクラス
    /// </summary>
    public partial class Thruster : IThruster
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equipment">装備情報</param>
        /// <param name="thrustStrafe">推進力</param>
        /// <param name="thrustPitch">推進力(ピッチ)</param>
        /// <param name="thrustYaw">推進力(ヨー)</param>
        /// <param name="thrustRoll">推進力(ロール)</param>
        /// <param name="angularRoll">角度(ロール)？</param>
        /// <param name="angularPitch">角度(ピッチ)？</param>
        public Thruster(
            IEquipment equipment,
            double thrustStrafe,
            double thrustPitch,
            double thrustYaw,
            double thrustRoll,
            double angularRoll,
            double angularPitch
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

            ThrustStrafe = thrustStrafe;
            ThrustPitch = thrustPitch;
            ThrustYaw = thrustYaw;
            ThrustRoll = thrustRoll;
            AngularRoll = angularRoll;
            AngularPitch = angularPitch;
        }
    }
}
