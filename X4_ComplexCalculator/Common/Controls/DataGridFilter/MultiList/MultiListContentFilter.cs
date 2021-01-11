using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.Common.Controls.DataGridFilter.Interface;
using X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions;

namespace X4_ComplexCalculator.Common.Controls.DataGridFilter.MultiList
{
    /// <summary>
    /// マルチリストフィルタ用クラス
    /// </summary>
    public class MultiListContentFilter : IDataGridFilter
    {
        #region プロパティ
        /// <summary>
        /// 除外された項目一覧
        /// </summary>
        public IReadOnlyList<string> ExcludedItems { get; }


        /// <inheritdoc/>
        public bool IsFilterEnabled => ExcludedItems.Any();
        #endregion


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="excludedItems">除外された項目一覧</param>
        public MultiListContentFilter(IEnumerable<string> excludedItems)
        {
            ExcludedItems = excludedItems.ToArray();
        }


        /// <inheritdoc/>
        public bool IsMatch(object? value)
        {
            if (value is not IEnumerable<string> enumerable)
            {
                return false;
            }

            if (!enumerable.Any() && !ExcludedItems.Contains(""))
            {
                return true;
            }

            foreach (var item in enumerable)
            {
                if (!ExcludedItems.Contains(item))
                {
                    return true;
                }
            }

            return false;
        }


        /// <inheritdoc/>
        public bool Equals(IContentFilter? other)
        {
            if (other is MultiListContentFilter filter)
            {
                return ExcludedItems.Count == filter.ExcludedItems.Count &&
                       ExcludedItems.OrderBy(x => x).SequenceEqual(filter.ExcludedItems.OrderBy(x => x));
            }

            return false;
        }
    }
}
