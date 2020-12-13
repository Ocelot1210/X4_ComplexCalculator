using System;
using System.ComponentModel;


namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support
{
    [Serializable()]
    public class FilterData : INotifyPropertyChanged
    {
        #region Metadata

        public FilterType Type { get; set; }
        public string ValuePropertyBindingPath { get; set; }
        public Type ValuePropertyType { get; set; }
        public bool IsTypeInitialized { get; set; }
        public bool IsCaseSensitiveSearch { get; set; }

        //query optimization fileds
        public bool IsSearchPerformed { get; set; }
        public bool IsRefresh { get; set; }
        //query optimization fileds
        #endregion

        #region Filter Change Notification
        public event EventHandler<EventArgs>? FilterChangedEvent;
        private bool _IsClearData;

        private void OnFilterChangedEvent()
        {
            var temp = FilterChangedEvent;

            if (temp != null)
            {
                bool filterChanged;
                switch (Type)
                {
                    case FilterType.Numeric:
                    case FilterType.DateTime:
                        filterChanged = (Operator != FilterOperator.Undefined || QueryString != String.Empty);
                        break;

                    case FilterType.NumericBetween:
                    case FilterType.DateTimeBetween:
                        _Operator = FilterOperator.Between;
                        filterChanged = true;
                        break;

                    case FilterType.Text:
                        _Operator = FilterOperator.Like;
                        filterChanged = true;
                        break;

                    case FilterType.List:
                    case FilterType.Boolean:
                        _Operator = FilterOperator.Equals;
                        filterChanged = true;
                        break;

                    default:
                        filterChanged = false;
                        break;
                }

                if (filterChanged && !_IsClearData)
                {
                    temp(this, EventArgs.Empty);
                }
            }
        }
        #endregion


        public void ClearData()
        {
            _IsClearData = true;

            Operator = FilterOperator.Undefined;
            QueryString = "";
            QueryStringTo = "";

            _IsClearData = false;
        }


        private FilterOperator _Operator = FilterOperator.Undefined;
        public FilterOperator Operator
        {
            get => _Operator;
            set
            {
                if (_Operator != value)
                {
                    _Operator = value;
                    NotifyPropertyChanged(nameof(Operator));
                    OnFilterChangedEvent();
                }
            }
        }

        private string _QueryString = "";
        public string QueryString
        {
            get => _QueryString;
            set
            {
                var val = (value is null) ? "" : value;
                if (val != value)
                {
                    _QueryString = val;
                    NotifyPropertyChanged(nameof(QueryString));
                    OnFilterChangedEvent();
                }
            }
        }


        private string _QueryStringTo = "";
        public string? QueryStringTo
        {
            get => _QueryStringTo;
            set
            {
                var val = (value is null) ? "" : value;
                if (val != value)
                {
                    _QueryStringTo = val;
                    NotifyPropertyChanged(nameof(QueryStringTo));
                    OnFilterChangedEvent();
                }
            }
        }


        public FilterData(
            FilterOperator @operator,
            FilterType type,
            string valuePropertyBindingPath,
            Type valuePropertyType,
            string queryString,
            string queryStringTo,
            bool isTypeInitialized,
            bool isCaseSensitiveSearch
            )
        {
            Operator = @operator;
            Type = type;
            ValuePropertyBindingPath = valuePropertyBindingPath;
            ValuePropertyType = valuePropertyType;
            QueryString   = queryString;
            QueryStringTo = queryStringTo;

            IsTypeInitialized = isTypeInitialized;
            IsCaseSensitiveSearch = isCaseSensitiveSearch;
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
