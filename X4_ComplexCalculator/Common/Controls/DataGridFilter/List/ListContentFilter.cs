using System;
using System.Collections.Generic;
using System.Linq;
using X4_ComplexCalculator.Common.Controls.DataGridFilter.Interface;
using X4_ComplexCalculator_CustomControlLibrary.DataGridExtensions;

namespace X4_ComplexCalculator.Common.Controls.DataGridFilter.List;

/// <summary>
/// リストフィルタ用クラス
/// </summary>
public class ListContentFilter : IDataGridFilter
{
    #region メンバ
    /// <summary>
    /// 除外された項目一覧
    /// </summary>
    private readonly IReadOnlyList<string> _excludedItems;
    #endregion


    #region プロパティ
    /// <inheritdoc/>
    public bool IsFilterEnabled => _excludedItems.Any();
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="excludedItems">除外された項目一覧</param>
    public ListContentFilter(IEnumerable<string> excludedItems)
    {
        _excludedItems = excludedItems.ToArray();
    }


    /// <inheritdoc/>
    public bool IsMatch(object? value)
    {
        return _excludedItems.Contains(value?.ToString() ?? string.Empty) != true;
    }


    /// <inheritdoc/>
    public bool Equals(IContentFilter? other)
    {
        if (other is ListContentFilter filter)
        {
            return _excludedItems.Count == filter._excludedItems.Count &&
                   _excludedItems.OrderBy(x => x).SequenceEqual(filter._excludedItems.OrderBy(x => x));
        }

        return false;
    }
}
