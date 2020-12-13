using System;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Querying
{
    public class FilteringEventArgs : EventArgs
    {
        public Exception Error { get; }

        public FilteringEventArgs(Exception ex)
        {
            Error = ex;
        }
    }
}
