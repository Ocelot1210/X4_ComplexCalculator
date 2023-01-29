using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Windows.Data;
using X4_ComplexCalculator.Main.WorkArea.WorkAreaData;

namespace X4_ComplexCalculator.Main.WorkArea.UI.StorageAssign;

/// <summary>
/// 保管庫割当用ViewModel
/// </summary>
public sealed class StorageAssignViewModel : BindableBase, IDisposable
{
    #region メンバ
    /// <summary>
    /// 保管庫割当用Model
    /// </summary>
    private readonly StorageAssignModel _model;
    #endregion


    #region プロパティ
    /// <summary>
    /// 保管庫割当情報
    /// </summary>
    public ListCollectionView StorageAssignInfo { get; }


    /// <summary>
    /// 指定時間
    /// </summary>
    public long Hour
    {
        get => _model.Hour;
        set => _model.Hour = value;
    }
    #endregion


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="stationData">計算機で使用するステーション情報</param>
    public StorageAssignViewModel(IStationData stationData)
    {
        _model = new StorageAssignModel(stationData.ProductsInfo, stationData.StoragesInfo, stationData.StorageAssignInfo);

        StorageAssignInfo = (ListCollectionView)CollectionViewSource.GetDefaultView(_model.StorageAssignGridItems);
        StorageAssignInfo.SortDescriptions.Clear();
        StorageAssignInfo.SortDescriptions.Add(new SortDescription(nameof(StorageAssignGridItem.Tier), ListSortDirection.Ascending));
        StorageAssignInfo.SortDescriptions.Add(new SortDescription(nameof(StorageAssignGridItem.WareName), ListSortDirection.Ascending));

        StorageAssignInfo.GroupDescriptions.Clear();
        StorageAssignInfo.GroupDescriptions.Add(new PropertyGroupDescription(nameof(StorageAssignGridItem.TransportTypeID)));
    }


    /// <summary>
    /// リソースを開放
    /// </summary>
    public void Dispose() => _model.Dispose();
}
