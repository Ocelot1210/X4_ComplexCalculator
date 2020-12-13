using System.Collections.Generic;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Querying
{
    public class Query
    {
        public string FilterString { get; set; } = "";

        public List<object?> QueryParameters { get; set; } = new();


        private string _LastFilterString = "";


        private List<object?> _LastQueryParameters = new();



        public bool IsQueryChanged
        {
            get
            {
                if (FilterString != _LastFilterString)
                {
                    return true;
                }

                if (QueryParameters.Count != _LastQueryParameters.Count)
                {
                    return true;
                }

                using var qe = QueryParameters.GetEnumerator();
                using var lqe = _LastQueryParameters.GetEnumerator();

                while (qe.MoveNext() && lqe.MoveNext())
                {
                    if (!qe.Current?.Equals(lqe.Current) == true)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void StoreLastUsedValues()
        {
            _LastFilterString    = FilterString;
            _LastQueryParameters = QueryParameters;
        }
    }
}
