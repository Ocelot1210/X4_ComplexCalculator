using System.Collections.Generic;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.DB.X4DB.Entity
{
    public partial class Ship
    {
        #region IWare
        /// <inheritdoc/>
        public string ID { get; }


        /// <inheritdoc/>
        public string Name { get; }


        /// <inheritdoc/>
        public IWareGroup WareGroup { get; }


        /// <inheritdoc/>
        public ITransportType TransportType { get; }


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
        public IReadOnlyList<IFaction> Owners { get; }


        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IWareProduction> Productions { get; }


        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IReadOnlyList<IWareResource>> Resources { get; }


        /// <inheritdoc/>
        public HashSet<string> Tags { get; }


        /// <inheritdoc/>
        public IWareEffects WareEffects { get; }
        #endregion


        #region IEquippableWare
        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IWareEquipment> Equipments { get; }
        #endregion


        #region IMacro
        /// <inheritdoc/>
        public string MacroName { get; }
        #endregion


        #region IShip
        /// <inheritdoc/>
        public IShipType ShipType { get; }


        /// <inheritdoc/>
        public IX4Size Size { get; }


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
        public IReadOnlyDictionary<string, IShipHanger> ShipHanger { get; }


        /// <inheritdoc/>
        public IReadOnlyDictionary<string, IReadOnlyList<IShipLoadout>> Loadouts { get; }
        #endregion
    }
}
