﻿using Prism.Mvvm;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collection;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewer.Ships;

/// <summary>
/// 艦船情報用ViewModel
/// </summary>
class ShipsViewModel : BindableBase
{
    #region メンバ
    /// <summary>
    /// ウェア一覧
    /// </summary>
    private readonly ObservableRangeCollection<ShipsGridItem> _ships = new();
    #endregion


    #region プロパティ
    /// <summary>
    /// 表示用データ
    /// </summary>
    public ListCollectionView ShipsView { get; }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public ShipsViewModel()
    {
        var items = X4Database.Instance.Ware.GetAll<IShip>()
            .Select(x => ShipsGridItem.Create(x))
            .Where(x => x is not null)
            .Select(x => x!);

        _ships = new(items);

        ShipsView = (ListCollectionView)CollectionViewSource.GetDefaultView(_ships);
        ShipsView.SortDescriptions.Clear();
        ShipsView.SortDescriptions.Add(new SortDescription(nameof(ShipsGridItem.ShipName), ListSortDirection.Ascending));
    }
}
