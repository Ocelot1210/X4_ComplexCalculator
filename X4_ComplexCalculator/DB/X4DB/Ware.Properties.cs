using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace X4_ComplexCalculator.DB.X4DB
{
    public partial class Ware
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
    }
}
