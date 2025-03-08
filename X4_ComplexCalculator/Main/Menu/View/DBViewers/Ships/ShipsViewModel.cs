using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using X4_ComplexCalculator.Common.Collections;
using X4_ComplexCalculator.DB;
using X4_ComplexCalculator.DB.X4DB.Interfaces;

namespace X4_ComplexCalculator.Main.Menu.View.DBViewers.Ships;

/// <summary>
/// 艦船情報用ViewModel
/// </summary>
sealed partial class ShipsViewModel : ObservableObject
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
