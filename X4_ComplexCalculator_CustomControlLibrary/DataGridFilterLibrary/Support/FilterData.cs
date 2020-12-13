﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support
{
    [Serializable()]
    public class FilterData : INotifyPropertyChanged
    {
        #region Metadata

        public FilterType Type { get; set; }
        public String ValuePropertyBindingPath { get; set; }
        public Type ValuePropertyType { get; set; }
        public bool IsTypeInitialized { get; set; }
        public bool IsCaseSensitiveSearch { get; set; }

        //query optimization fileds
        public bool IsSearchPerformed { get; set; }
        public bool IsRefresh { get; set; }
        //query optimization fileds
        #endregion

        #region Filter Change Notification
        public event EventHandler<EventArgs> FilterChangedEvent;
        private bool isClearData;

        private void OnFilterChangedEvent()
        {
            EventHandler<EventArgs> temp = FilterChangedEvent;

            if (temp != null)
            {
                bool filterChanged = false;

                switch (Type)
                {
                    case FilterType.Numeric:
                    case FilterType.DateTime:

                        filterChanged = Operator != FilterOperator.Undefined || QueryString != String.Empty;
                        break;

                    case FilterType.NumericBetween:
                    case FilterType.DateTimeBetween:

                        _operator = FilterOperator.Between;
                        filterChanged = true;
                        break;

                    case FilterType.Text:

                        _operator = FilterOperator.Like;
                        filterChanged = true;
                        break;

                    case FilterType.List:
                    case FilterType.Boolean:

                        _operator = FilterOperator.Equals;
                        filterChanged = true;
                        break;

                    default:
                        filterChanged = false;
                        break;
                }

                if (filterChanged && !isClearData) temp(this, EventArgs.Empty);
            }
        }
        #endregion
        public void ClearData()
        {
            isClearData = true;

            Operator           = FilterOperator.Undefined;
            if (QueryString   != String.Empty) QueryString = null;
            if (QueryStringTo != String.Empty) QueryStringTo = null;

            isClearData = false;
        }

        private FilterOperator _operator;
        public FilterOperator Operator
        {
            get { return _operator; }
            set
            {
                if(_operator != value)
                {
                    _operator = value;
                    NotifyPropertyChanged(nameof(Operator));
                    OnFilterChangedEvent();
                }
            }
        }

        private string queryString;
        public string QueryString
        {
            get { return queryString; }
            set
            {
                if (queryString != value)
                {
                    queryString = value ?? String.Empty;

                    NotifyPropertyChanged(nameof(QueryString));
                    OnFilterChangedEvent();
                }
            }
        }

        private string queryStringTo;
        public string QueryStringTo
        {
            get { return queryStringTo; }
            set
            {
                if (queryStringTo != value)
                {
                    queryStringTo = value ?? String.Empty;

                    NotifyPropertyChanged(nameof(QueryStringTo));
                    OnFilterChangedEvent();
                }
            }
        }

        public FilterData(
            FilterOperator Operator,
            FilterType Type,
            String ValuePropertyBindingPath,
            Type ValuePropertyType,
            String QueryString,
            String QueryStringTo,
            bool IsTypeInitialized,
            bool IsCaseSensitiveSearch
            )
        {
            this.Operator = Operator;
            this.Type = Type;
            this.ValuePropertyBindingPath = ValuePropertyBindingPath;
            this.ValuePropertyType = ValuePropertyType;
            this.QueryString   = QueryString;
            this.QueryStringTo = QueryStringTo;

            this.IsTypeInitialized    = IsTypeInitialized;
            this.IsCaseSensitiveSearch = IsCaseSensitiveSearch;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
