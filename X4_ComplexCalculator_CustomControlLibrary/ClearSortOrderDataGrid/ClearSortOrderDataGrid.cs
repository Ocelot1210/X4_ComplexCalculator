using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace X4_ComplexCalculator_CustomControlLibrary;

/// <summary>
/// ソート順をクリアしても列ヘッダに三角マークが残らないDataGrid
/// </summary>
public class ClearSortOrderDataGrid : DataGrid
{
    #region メンバ
    /// <summary>
    /// ICollectionView.SortDescriptionsの前回値
    /// </summary>
    private INotifyCollectionChanged? _sortDescriptions;
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    static ClearSortOrderDataGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ClearSortOrderDataGrid), new FrameworkPropertyMetadata(typeof(ClearSortOrderDataGrid)));
    }


    /// <summary>
    /// ItemsSource変更時
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);

        bool clearPrevValueNeeded = true;       // 前回値をクリアする必要があるか

        var view = CollectionViewSource.GetDefaultView(newValue);
        if (view != null)
        {
            if (view.SortDescriptions is INotifyCollectionChanged collection)
            {
                if (_sortDescriptions != null)
                {
                    _sortDescriptions.CollectionChanged -= SortDescription_CollectionChanged;
                }

                collection.CollectionChanged += SortDescription_CollectionChanged;

                _sortDescriptions = collection;
                clearPrevValueNeeded = false;
            }
        }

        // 前回値クリアが必要ならクリアする
        if (clearPrevValueNeeded && _sortDescriptions != null)
        {
            _sortDescriptions.CollectionChanged -= SortDescription_CollectionChanged;
            _sortDescriptions = null;
        }
    }


    /// <summary>
    /// ソート順変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SortDescription_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not SortDescriptionCollection collection)
        {
            return;
        }

        // ソート順が空なら矢印を消す
        if (collection.Count == 0)
        {
            foreach (var column in Columns)
            {
                column.SortDirection = null;
            }
        }
    }
}
