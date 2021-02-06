using System.Collections.Generic;

namespace X4_ComplexCalculator.DB.X4DB
{
    public partial class Ship
    {
        #region IWare
        /// <inheritdoc/>
        public string ID { get; }


        /// <inheritdoc/>
        public string Name { get; }


        /// <inheritdoc/>
        public WareGroup WareGroup { get; }


        /// <inheritdoc/>
        public TransportType TransportType { get; }


        /// <inheritdoc/>
        public string Description { get; }


        /// <inheritdoc/>
        public long Volume { get; }


        /// <inheritdoc/>
        public long MinPrice { get; }


        /// <inheritdoc/>
        public long AvgPrice { get; }


        /// <inheritdoc/>
        public long MaxPrice { get; }


        /// <inheritdoc/>
        public IReadOnlyList<Faction> Owners { get; }


        /// <inheritdoc/>
        public IReadOnlyDictionary<string, WareProduction> Productions { get; }


        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IReadOnlyList<WareResource>> Resources { get; }


        /// <inheritdoc/>
        public HashSet<string> Tags { get; }


        /// <inheritdoc/>
        public WareEffects WareEffects { get; }
        #endregion



        #region IEquippableWare
        /// <inheritdoc/>
        public IReadOnlyDictionary<string, WareEquipment> Equipments { get; }
        #endregion



        #region IShip
        /// <inheritdoc/>
        public ShipType ShipType { get; }


        /// <inheritdoc/>
        public string Macro { get; }


        /// <inheritdoc/>
        public X4Size Size { get; }


        /// <inheritdoc/>
        public double Mass { get; }


        /// <inheritdoc/>
        public Drag Drag { get; }


        /// <inheritdoc/>
        public Inertia Inertia { get; }


        /// <inheritdoc/>
        public long Hull { get; }


        /// <inheritdoc/>
        public long People { get; }


        /// <inheritdoc/>
        public long MissileStorage { get; }


        /// <inheritdoc/>
        public long DroneStorage { get; }


        /// <inheritdoc/>
        public long CargoSize { get; }


        /// <inheritdoc/>
        public IReadOnlyDictionary<string, ShipHanger> ShipHanger { get; }


        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IReadOnlyList<ShipLoadout>> Loadouts { get; }
        #endregion
    }
}
