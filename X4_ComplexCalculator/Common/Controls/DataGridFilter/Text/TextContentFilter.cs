using System;
using X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions;

namespace X4_ComplexCalculator.Common.Controls.DataGridFilter.Text
{
    /// <summary>
    /// 文字列フィルタ用クラス
    /// </summary>
    class TextContentFilter : IContentFilter
    {
        /// <summary>
        /// フィルタ文字列
        /// </summary>
        public string FilterText { get; }


        /// <summary>
        /// 一致条件
        /// </summary>
        public TextFilterConditions Conditions { get; }


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="filterText">フィルタ文字列</param>
        /// <param name="condition">フィルタ条件</param>
        /// <param name="excludedItems"></param>
        public TextContentFilter(string filterText, TextFilterConditions conditions)
        {
            FilterText = filterText;
            Conditions = conditions;
        }


        /// <inheritdoc/>
        public bool IsMatch(object? value)
        {
            // フィルタが空なら無条件で通す
            if (FilterText == "")
            {
                return true;
            }

            var valStr = value?.ToString() ?? "";
            var ret = Conditions switch
            {
                TextFilterConditions.Contains =>     valStr.Contains(FilterText,   StringComparison.InvariantCultureIgnoreCase),
                TextFilterConditions.NotContains => !valStr.Contains(FilterText,   StringComparison.InvariantCultureIgnoreCase),
                TextFilterConditions.Equals =>       valStr.Equals(FilterText,     StringComparison.InvariantCultureIgnoreCase),
                TextFilterConditions.NotEquals =>   !valStr.Equals(FilterText,     StringComparison.InvariantCultureIgnoreCase),
                TextFilterConditions.StartWith =>    valStr.StartsWith(FilterText, StringComparison.InvariantCultureIgnoreCase),
                TextFilterConditions.EndWith =>      valStr.EndsWith(FilterText,   StringComparison.InvariantCultureIgnoreCase),
                _ => throw new NotImplementedException(),
            };

            return ret;
        }
    }
}
