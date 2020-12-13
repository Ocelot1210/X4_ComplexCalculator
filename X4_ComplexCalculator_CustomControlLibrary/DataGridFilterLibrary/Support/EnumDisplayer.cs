using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Collections;
using System.Reflection;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Markup;

namespace X4_ComplexCalculator_CustomControlLibrary.DataGridFilterLibrary.Support
{
    /// <summary>
    /// Code from: http://www.ageektrapped.com/blog/the-missing-net-7-displaying-enums-in-wpf/
    /// </summary>
    [ContentProperty("OverriddenDisplayEntries")]
    public class EnumDisplayer : IValueConverter
    {
        private Type _Type;
        private IDictionary? _DisplayValues;
        private IDictionary? _RreverseValues;


        private List<EnumDisplayEntry> _OverriddenDisplayEntries = new();
        public List<EnumDisplayEntry> OverriddenDisplayEntries
        {
            get
            {
                if (_OverriddenDisplayEntries == null)
                {
                    _OverriddenDisplayEntries = new List<EnumDisplayEntry>();
                }
                return _OverriddenDisplayEntries;
            }
        }


        public EnumDisplayer(Type type)
        {
            if (!type.IsEnum)
            {
                throw new ArgumentException("parameter is not an Enumermated type", "value");
            }
            _Type = type;
        }

        public Type Type
        {
            get { return _Type; }
            set
            {
                if (!value.IsEnum)
                {
                    throw new ArgumentException("parameter is not an Enumermated type", "value");
                }
                _Type = value;
            }
        }

        public ReadOnlyCollection<string> DisplayNames
        {
            get
            {
                Type displayValuesType = typeof(Dictionary<,>).GetGenericTypeDefinition().MakeGenericType(_Type, typeof(string));

                _DisplayValues = (IDictionary?)Activator.CreateInstance(displayValuesType);

                _RreverseValues = (IDictionary?)Activator.CreateInstance(typeof(Dictionary<,>)
                                                         .GetGenericTypeDefinition()
                                                         .MakeGenericType(typeof(string), _Type));

                var fields = _Type.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    DisplayStringAttribute[] a = (DisplayStringAttribute[])
                                                field.GetCustomAttributes(typeof(DisplayStringAttribute), false);

                    string? displayString = GetDisplayStringValue(a);
                    object enumValue = field.GetValue(null)!;

                    if (displayString == null)
                    {
                        displayString = GetBackupDisplayStringValue(enumValue);
                    }

                    if (displayString != null)
                    {
                        _DisplayValues?.Add(enumValue, displayString);
                        _RreverseValues?.Add(displayString, enumValue);
                    }
                }

                return new List<string>((IEnumerable<string>)_DisplayValues?.Values!).AsReadOnly();
            }
        }

        private string? GetDisplayStringValue(DisplayStringAttribute[] attr)
        {
            if (attr == null || attr.Length == 0)
            {
                return null;
            }

            DisplayStringAttribute dsa = attr[0];
            if (!string.IsNullOrEmpty(dsa.ResourceKey))
            {
                return new ResourceManager(_Type).GetString(dsa.ResourceKey);
            }

            return dsa.Value;
        }

        private string? GetBackupDisplayStringValue(object enumValue)
        {
            if (_OverriddenDisplayEntries != null && _OverriddenDisplayEntries.Count > 0)
            {
                var foundEntry = _OverriddenDisplayEntries.Find(entry =>
                {
                    var e = Enum.Parse(_Type, entry.EnumValue);
                    return enumValue.Equals(e);
                });

                if (foundEntry != null)
                {
                    if (foundEntry.ExcludeFromDisplay)
                    {
                        return null;
                    }
                    return foundEntry.DisplayString;
                }
            }

            return Enum.GetName(_Type, enumValue);
        }


        object? IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _DisplayValues?[value];
        }

        object? IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _RreverseValues?[value];
        }
    }

    public class EnumDisplayEntry
    {
        public string EnumValue { get; set; } = "";
        public string DisplayString { get; set; } = "";
        public bool ExcludeFromDisplay { get; set; }
    }
}
