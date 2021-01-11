using System;
using X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions;

namespace X4_ComplexCalculator.Common.Controls.DataGridFilter.Interface
{
    public interface IDataGridFilter : IContentFilter, IEquatable<IContentFilter>
    {
        /// <summary>
        /// フィルタが有効か
        /// </summary>
        public bool IsFilterEnabled { get; }
    }
}
