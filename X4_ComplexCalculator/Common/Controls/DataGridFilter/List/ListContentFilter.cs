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
    private readonly IReadOnlyList<string> _ExcludedItems;
    #endregion


    #region プロパティ
    /// <inheritdoc/>
    public bool IsFilterEnabled => _ExcludedItems.Any();
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="excludedItems">除外された項目一覧</param>
    public ListContentFilter(IEnumerable<string> excludedItems)
    {
        _ExcludedItems = excludedItems.ToArray();
    }


    /// <inheritdoc/>
    public bool IsMatch(object? value)
    {
        return _ExcludedItems.Contains(value?.ToString() ?? string.Empty) != true;
    }


    /// <inheritdoc/>
    public bool Equals(IContentFilter? other)
    {
        if (other is ListContentFilter filter)
        {
            return _ExcludedItems.Count == filter._ExcludedItems.Count &&
                   _ExcludedItems.OrderBy(x => x).SequenceEqual(filter._ExcludedItems.OrderBy(x => x));
        }

        return false;
    }
}
