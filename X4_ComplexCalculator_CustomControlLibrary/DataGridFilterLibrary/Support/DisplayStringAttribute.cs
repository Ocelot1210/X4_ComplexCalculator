using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support
{
    /// <summary>
    /// Code from: http://www.ageektrapped.com/blog/the-missing-net-7-displaying-enums-in-wpf/
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DisplayStringAttribute : Attribute
    {
        private readonly string value;
        public string Value => value;

        public string ResourceKey { get; set; }

        public DisplayStringAttribute(string v)
        {
            this.value = v;
        }

        public DisplayStringAttribute()
        {
        }
    }
}
