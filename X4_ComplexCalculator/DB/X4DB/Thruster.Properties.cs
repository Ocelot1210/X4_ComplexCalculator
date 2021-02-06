﻿using System;
using System.Collections.Generic;
using System.Text;

namespace X4_ComplexCalculator.DB.X4DB
{
    public partial class Thruster
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


        #region IEquipment
        /// <inheritdoc/>
        public string Macro { get; }

        /// <inheritdoc/>
        public EquipmentType EquipmentType { get; }

        /// <inheritdoc/>
        public long Hull { get; }

        /// <inheritdoc/>
        public bool HullIntegrated { get; }

        /// <inheritdoc/>
        public long Mk { get; }

        /// <inheritdoc/>
        public Race? MakerRace { get; }

        /// <inheritdoc/>
        public HashSet<string> EquipmentTags { get; }

        /// <inheritdoc/>
        public X4Size? Size { get; }
        #endregion


        #region IThruster
        /// <inheritdoc/>
        public double ThrustStrafe { get; }


        /// <inheritdoc/>
        public double ThrustPitch { get; }


        /// <inheritdoc/>
        public double ThrustYaw { get; }


        /// <inheritdoc/>
        public double ThrustRoll { get; }


        /// <inheritdoc/>
        public double AngularRoll { get; }


        /// <inheritdoc/>
        public double AngularPitch { get; }
        #endregion
    }
}
